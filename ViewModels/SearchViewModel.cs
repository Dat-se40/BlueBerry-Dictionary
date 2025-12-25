using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using BlueBerryDictionary.Views.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDictionary.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace BlueBerryDictionary.ViewModels
{
    public partial class SearchViewModel : ObservableObject
    {
        private readonly WordSearchService _searchService;
        private CancellationTokenSource _searchCts;
        private INavigationService _navigationService;
        private DetailsPage _detailsPage;
        #region Observable properties

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private string _searchButtonText = "Search";

        [ObservableProperty]
        private ObservableCollection<string> _suggestions;

        [ObservableProperty]
        private bool _isSuggestionsOpen;

        [ObservableProperty]
        private List<Word> _currentWords;

        [ObservableProperty]
        private bool _isSearching;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private bool _hasResults;

        [ObservableProperty]
        private string _usAudioUrl;

        [ObservableProperty]
        private string _ukAudioUrl;
        public ICommand SearchFromRelatedWordCommand { get; }
        #endregion

        #region Constructor
        public SearchViewModel(INavigationService navigationService)
        {
            _searchService = new WordSearchService();
            _suggestions = new ObservableCollection<string>();
            _navigationService = navigationService;


            // Thêm command cho các từ đồng nghĩa trái nghĩa
            SearchFromRelatedWordCommand = new RelayCommand<string>(async (word) =>
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    _ = SearchAndNavigate(word); 
                    
                }
            });

        }

        #endregion

        #region Property changed handlers

        partial void OnSearchTextChanged(string value)
        {
            OnSearchTextChangedAsync();
        }

        /// <summary>
        /// Debounce search với autocomplete
        /// </summary>
        private async void OnSearchTextChangedAsync()
        {
            // Cancel previous search
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Suggestions.Clear();
                IsSuggestionsOpen = false;
                return;
            }

            try
            {
                // Debounce 300ms
                await Task.Delay(300, _searchCts.Token);

                // Get autocomplete suggestions
                var suggestions = _searchService.GetSuggestions(SearchText, 5);
                Suggestions.Clear();
                foreach (var suggestion in suggestions)
                {
                    Suggestions.Add(suggestion);
                }

                IsSuggestionsOpen = Suggestions.Count > 0;
            }
            catch (TaskCanceledException)
            {
                // Expected when user types fast
            }
        }
        #endregion

        #region Search logic

        /// <summary>
        /// Tìm từ và navigate tới DetailsPage
        /// </summary>
        private async Task SearchAndNavigate(string word)
        {
            SearchText = word;
            await ExecuteSearchAsync();

            if (HasResults && CurrentWords != null && CurrentWords.Count > 0)
            {
                var detailsPage = new DetailsPage(CurrentWords[0], OnWordClicked);
                detailsPage.DataContext = this;

                string uniqueId = $"Details_{word}_{DateTime.Now.Ticks}";
                _navigationService.NavigateTo("Details", detailsPage, uniqueId);
            }
        }

        /// <summary>
        /// Callback khi click từ liên quan
        /// </summary>
        public async void OnWordClicked(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return;
            await SearchAndNavigate(word);
        }

        #endregion

        #region Commands

        [RelayCommand] 
        public async Task ExcuteSearchAndNavigate() 
        {
            await SearchAndNavigate(SearchText);
        }
        /// <summary>
        /// Execute full search
        /// </summary>
        [RelayCommand]
        public async Task ExecuteSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                StatusMessage = "Please enter a word to search";
                return;
            }

            SearchButtonText = "Searching...";
            IsSearching = true;
            HasResults = false;
            IsSuggestionsOpen = false;

            try
            {
                _searchCts?.Cancel();
                _searchCts = new CancellationTokenSource();

                // Parallel execution: search word + get audio
                var searchTask = _searchService.SearchWordAsync(SearchText, _searchCts.Token);
                var audioTask = _searchService.GetAudioAsync(SearchText);

                await Task.WhenAll(searchTask, audioTask);

                var words = searchTask.Result;
                var (usAudio, ukAudio) = audioTask.Result;

                if (words != null && words.Count > 0)
                {
                    CurrentWords = words;
                    HasResults = true;
                    StatusMessage = $"Found definition for '{SearchText}'";
                    UsAudioUrl = usAudio;
                    UkAudioUrl = ukAudio;
                }
                else
                {
                    CurrentWords = null;
                    StatusMessage = $"No results found for '{SearchText}'";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                CurrentWords = null;
            }
            finally
            {
                IsSearching = false;
                SearchButtonText = "Search";
            }

            Console.WriteLine(StatusMessage);
        }

        
        /// <summary>
        /// Clear search
        /// </summary>
        [RelayCommand]
        private void ExecuteClear()
        {
            SearchText = string.Empty;
            CurrentWords = null;
            Suggestions.Clear();
            IsSuggestionsOpen = false;
            HasResults = false;
            StatusMessage = string.Empty;
            UsAudioUrl = null;
            UkAudioUrl = null;
        }

        /// <summary>
        /// Select suggestion
        /// </summary>
        [RelayCommand]
        private async Task ExecuteSelectSuggestion(string suggestion)
        {
            SearchText = suggestion;
            IsSuggestionsOpen = false;
            await ExecuteSearchAsync();
        }

        /// <summary>
        /// Phát âm US
        /// </summary>
        [RelayCommand]
        private async Task PlayUsAudio()
        {
            Console.WriteLine("OK" + CurrentWords[0].phonetic);
            await PlayAudioAsync(UsAudioUrl);
        }

        /// <summary>
        /// Phát âm UK
        /// </summary>
        [RelayCommand]
        private async Task PlayUkAudio()
        {
            await PlayAudioAsync(UkAudioUrl);
        }

        /// <summary>
        /// Download từ về local storage
        /// </summary>
        [RelayCommand]
        private async Task ExecuteDownload()
        {
            if (CurrentWords == null || CurrentWords.Count == 0) return;

            var word = CurrentWords[0].word;
            var wordPath = Data.FileStorage.GetWordFilePath(word);
            bool already = File.Exists(wordPath);

            if (!already)
                Data.FileStorage.Download(CurrentWords);
            var imageService = MyDictionary.Services.ImageSearchService.Instance;
            await imageService.EnsureImageDownloadedAsync(word);

            MessageBox.Show(already ? "This word has been downloaded" : "he word and images have been downloaded to your device");
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Load audio URLs for pronunciation
        /// </summary>
        private async Task LoadAudioAsync(string word)
        {
            try
            {
                var (usAudio, ukAudio) = await _searchService.GetAudioAsync(word);
                UsAudioUrl = usAudio;
                UkAudioUrl = ukAudio;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load audio: {ex.Message}");
            }
        }

        /// <summary>
        /// Phát audio sử dụng Audio service
        /// </summary>
        private async Task PlayAudioAsync(string audioUrl)
        {
            if (string.IsNullOrEmpty(audioUrl))
            {
                StatusMessage = "No audio available";
                return;
            }

            try
            {
                var audioService = new ApiClient.Client.Audio();
                await audioService.PlayAudioAsync(audioUrl);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Audio error: {ex.Message}";
            }
        }
        #endregion
    }
}