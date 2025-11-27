using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace BlueBerryDictionary.ViewModels
{
    public partial class MyWordsViewModel : ObservableObject
    {
        private readonly TagService _tagService;

        // ========== OBSERVABLE PROPERTIES ==========

        [ObservableProperty]
        private ObservableCollection<Tag> _tags;

        [ObservableProperty]
        private ObservableCollection<WordShortened> _filteredWords;

        [ObservableProperty]
        private Tag _selectedTag;

        [ObservableProperty]
        private string _selectedLetter = "ALL";

        [ObservableProperty]
        private string _selectedPartOfSpeech;

        [ObservableProperty]
        private int _totalWords;

        [ObservableProperty]
        private int _totalTags;

        [ObservableProperty]
        private int _wordsThisWeek;

        [ObservableProperty]
        private int _wordsThisMonth;

        [ObservableProperty]
        private ObservableCollection<AlphabetItem> _alphabetItems;

        // ========== CONSTRUCTOR ==========

        public MyWordsViewModel()
        {
            _tagService = TagService.Instance;

            Tags = new ObservableCollection<Tag>();
            FilteredWords = new ObservableCollection<WordShortened>();
            AlphabetItems = new ObservableCollection<AlphabetItem>();

            LoadData();
        }

        // ========== DATA LOADING ==========

        private void LoadData()
        {
            // Load tags
            Tags.Clear();
            foreach (var tag in _tagService.GetAllTags())
            {
                Tags.Add(tag);
            }

            // Load statistics
            UpdateStatistics();

            // Load alphabet distribution
            LoadAlphabetDistribution();

            // Load all words initially
            ApplyFilters();
        }

        private void LoadAlphabetDistribution()
        {
            AlphabetItems.Clear();

            var distribution = _tagService.GetLetterDistribution();

            // Add "All" button
            AlphabetItems.Add(new AlphabetItem
            {
                Letter = "ALL",
                WordCount = _tagService.GetTotalWords(),
                IsActive = true,
                IsAvailable = true
            });

            // Add A-Z
            for (char c = 'A'; c <= 'Z'; c++)
            {
                var letter = c.ToString();
                var count = distribution.GetValueOrDefault(letter, 0);

                AlphabetItems.Add(new AlphabetItem
                {
                    Letter = letter,
                    WordCount = count,
                    IsActive = false,
                    IsAvailable = count > 0
                });
            }
        }

        private void UpdateStatistics()
        {
            TotalWords = _tagService.GetTotalWords();
            TotalTags = _tagService.GetTotalTags();
            WordsThisWeek = _tagService.GetWordsAddedThisWeek();
            WordsThisMonth = _tagService.GetWordsAddedThisMonth();
        }

        // ========== FILTERING ==========

        private void ApplyFilters()
        {
            var words = _tagService.GetAllWords();

            // Filter by tag
            if (SelectedTag != null)
            {
                words = _tagService.GetWordsByTag(SelectedTag.Id);
            }

            // Filter by letter
            if (!string.IsNullOrEmpty(SelectedLetter) && SelectedLetter != "ALL")
            {
                words = words.Where(w => w.Word.StartsWith(SelectedLetter, System.StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Filter by part of speech
            if (!string.IsNullOrEmpty(SelectedPartOfSpeech))
            {
                words = words.Where(w => w.PartOfSpeech.Equals(SelectedPartOfSpeech, System.StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Update UI
            FilteredWords.Clear();
            foreach (var word in words)
            {
                FilteredWords.Add(word);
            }
        }

        // ========== RELAY COMMANDS ==========

        [RelayCommand]
        private void FilterByTag(Tag tag)
        {
            SelectedTag = tag;
            ApplyFilters();
        }

        [RelayCommand]
        private void FilterByLetter(string letter)
        {
            SelectedLetter = letter;

            // Update alphabet buttons state
            foreach (var item in AlphabetItems)
            {
                item.IsActive = item.Letter == letter;
            }

            ApplyFilters();
        }

        [RelayCommand]
        private void FilterByPartOfSpeech(string pos)
        {
            SelectedPartOfSpeech = pos;
            ApplyFilters();
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedTag = null;
            SelectedLetter = "ALL";
            SelectedPartOfSpeech = null;

            // Reset alphabet buttons
            FilterByLetter("ALL");

            ApplyFilters();
        }

        [RelayCommand]
        private void CreateTag()
        {
            // TODO: Show dialog to input tag name, icon, color
            var tagName = "New Tag"; // Get from dialog
            _tagService.CreateTag(tagName);
            LoadData();
        }

        [RelayCommand]
        private void DeleteTag(string tagId)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa tag này? Các từ sẽ không bị xóa.",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                _tagService.DeleteTag(tagId);
                LoadData();
            }
        }

        [RelayCommand]
        private void AddWord()
        {
            // TODO: Show dialog to add new word
            MessageBox.Show("Chức năng thêm từ mới đang phát triển");
        }

        [RelayCommand]
        private void DeleteWord(string word)
        {
            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa từ '{word}'?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                _tagService.RemoveWord(word);
                LoadData();
            }
        }

        [RelayCommand]
        private void AddTagToWord(object[] parameters)
        {
            if (parameters.Length < 2) return;

            var word = parameters[0] as string;
            var tagId = parameters[1] as string;

            if (word != null && tagId != null)
            {
                _tagService.AddTagToWord(word, tagId);
                LoadData();
            }
        }

        [RelayCommand]
        private void ViewWordDetails(string word)
        {
            // TODO: Navigate to DetailsPage
            MessageBox.Show($"View details for: {word}");
        }
    }

    // ========== HELPER CLASSES ==========

    public partial class AlphabetItem : ObservableObject
    {
        [ObservableProperty]
        private string _letter;

        [ObservableProperty]
        private int _wordCount;

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private bool _isAvailable;

        public string DisplayText => WordCount > 0 ? $"{Letter} ({WordCount})" : Letter;
    }
}