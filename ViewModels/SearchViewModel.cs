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
        // ==================== SERVICES ====================
        private readonly WordSearchService _searchService;
        private CancellationTokenSource _searchCts;
        private INavigationService _navigationService;
        private DetailsPage _detailsPage;

        // ==================== OBSERVABLE PROPERTIES ====================

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private string _searchButtonText = "Tìm kiếm";

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

        // ==================== CONSTRUCTOR ====================
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
                    SearchText = word;
                    await ExecuteSearchAsync();
                }
            });

        }

        // ==================== PROPERTY CHANGED HANDLERS ====================

        partial void OnSearchTextChanged(string value)
        {
            OnSearchTextChangedAsync();
        }

        partial void OnCurrentWordsChanged(List<Word> value)
        {
            if (value != null && value.Count > 0)
            {
                _detailsPage = new DetailsPage(value[0], OnWordClicked);
                _detailsPage.DataContext = this;
                _navigationService.Navigate(_detailsPage, _detailsPage.ToString() + $" {CurrentWords[0].word}");
            }
        }
        public async void OnWordClicked(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return;

            SearchText = word;
            await ExecuteSearchAsync();
        }
        // ==================== SEARCH LOGIC ====================

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

        // ==================== RELAY COMMANDS ====================

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
            SearchButtonText = "Đang tìm...";

            IsSearching = true;
            HasResults = false;
            StatusMessage = $"Searching for '{SearchText}'...";
            IsSuggestionsOpen = false;
            Console.WriteLine(StatusMessage);
            try
            {
                // Cancel any ongoing search
                _searchCts?.Cancel();
                _searchCts = new CancellationTokenSource();

                // Search word
                var words = await _searchService.SearchWordAsync(SearchText, _searchCts.Token);

                if (words != null && words.Count > 0)
                {
                    CurrentWords = words; // <- nếu cùng bấm lại từ này ở trang khác thì ko load được vì ko có OnChanged
                    // Check thêm Frame.Content != Frame.Previous content hay ko?
                    
                    HasResults = true;
                    StatusMessage = $"Found definition for '{SearchText}'";
                    _navigationService.Navigate(_detailsPage, _detailsPage.ToString() + $" {CurrentWords}");
                    // Load audio URLs
                    await LoadAudioAsync(SearchText);
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
                SearchButtonText = "Tìm kiếm";
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
        /// Play US audio
        /// </summary>
        [RelayCommand]
        private async Task PlayUsAudio()
        {
            Console.WriteLine("OKe" + CurrentWords[0].phonetic);
            await PlayAudioAsync(UsAudioUrl);
        }

        /// <summary>
        /// Play UK audio
        /// </summary>
        [RelayCommand]
        private async Task PlayUkAudio()
        {
            await PlayAudioAsync(UkAudioUrl);
        }

        /// <summary>
        /// Download word to local storage
        /// </summary>
        [RelayCommand]
        private void ExecuteDownload()
        {
            if (CurrentWords != null && CurrentWords.Count > 0)
            {
                if (File.Exists(Data.FileStorage.GetWordFilePath(CurrentWords[0].word)))
                {
                    MessageBox.Show("Từ này đã được tải");
                }
                else
                    Data.FileStorage.Download(CurrentWords);
            }
        }

        // ==================== HELPER METHODS ====================

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
        /// Play audio using Audio service
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
    }
}