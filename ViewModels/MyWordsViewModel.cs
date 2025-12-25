using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using BlueBerryDictionary.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BlueBerryDictionary.ViewModels
{
    public partial class MyWordsViewModel : ObservableObject
    {
        private readonly TagService _tagService;
        public Action acOnFilterWordsChanged;
        public Action acOnTagChanged; 
        // ========== OBSERVABLE PROPERTIES ==========
        [ObservableProperty]
        private string currFilter = "All"; // "All" for default

        [ObservableProperty]
        private int wordsCount;  

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
            _tagService.OnWordsChanged += UpdateStatistics; 
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

        public  void UpdateStatistics()
        {
            TotalWords = _tagService.GetTotalWords();
            TotalTags = _tagService.GetTotalTags();
            WordsThisWeek = _tagService.GetWordsAddedThisWeek();
            WordsThisMonth = _tagService.GetWordsAddedThisMonth();
            WordsCount =  FilteredWords.Count;
        }

        // ========== FILTERING ==========

        private void ApplyFilters()
        {
            Console.WriteLine($"🔍 ApplyFilters START");
            Console.WriteLine($"   SelectedTag: {SelectedTag?.Name ?? "null"}");
            Console.WriteLine($"   SelectedLetter: {SelectedLetter}");
            Console.WriteLine($"   SelectedPartOfSpeech: {SelectedPartOfSpeech}");

            // ✅ Bắt đầu từ TẤT CẢ từ (bao gồm cả favorite và có tag)
            var words = _tagService.GetAllWords();
            Console.WriteLine($"   Initial words: {words.Count}");

            var filterParts = new List<string>(); // Để build CurrFilter

            // ========================================
            // ✅ FILTER 1: TAG (nếu có)
            // ========================================
            if (SelectedTag != null)
            {
                // Lấy words thuộc tag này
                var tagWords = _tagService.GetWordsByTag(SelectedTag.Id);
                Console.WriteLine($"   Tag filter: {tagWords.Count} words in tag '{SelectedTag.Name}'");

                // INTERSECT: chỉ giữ từ có trong cả 2 lists
                words = words.Where(w => tagWords.Any(tw => tw.Word.Equals(w.Word, StringComparison.OrdinalIgnoreCase))).ToList();
                filterParts.Add(SelectedTag.Name);
                Console.WriteLine($"   After tag filter: {words.Count}");
            }

            // ========================================
            // ✅ FILTER 2: LETTER (nếu có)
            // ========================================
            if (!string.IsNullOrEmpty(SelectedLetter) && SelectedLetter != "ALL")
            {
                words = words.Where(w => w.Word.StartsWith(SelectedLetter, StringComparison.OrdinalIgnoreCase)).ToList();
                filterParts.Add(SelectedLetter.ToUpper());
                Console.WriteLine($"   After letter filter '{SelectedLetter}': {words.Count}");
            }

            // ========================================
            // ✅ FILTER 3: PART OF SPEECH (nếu có)
            // ========================================
            if (!string.IsNullOrEmpty(SelectedPartOfSpeech))
            {
                words = words.Where(w => w.PartOfSpeech.Equals(SelectedPartOfSpeech, StringComparison.OrdinalIgnoreCase)).ToList();
                filterParts.Add(SelectedPartOfSpeech.ToUpper());
                Console.WriteLine($"   After POS filter '{SelectedPartOfSpeech}': {words.Count}");
            }

            // ========================================
            // ✅ BUILD CURRFILTER TEXT
            // ========================================
            CurrFilter = filterParts.Count > 0 ? string.Join(" + ", filterParts) : "All";

            // ========================================
            // ✅ UPDATE UI
            // ========================================
            FilteredWords.Clear();
            foreach (var word in words)
            {
                FilteredWords.Add(word);
            }

            WordsCount = FilteredWords.Count;
            Console.WriteLine($"🔍 ApplyFilters END: {WordsCount} words, CurrFilter='{CurrFilter}'");

            acOnFilterWordsChanged?.Invoke();
        }

        // ========== RELAY COMMANDS ==========
        [RelayCommand]
        private void OpenRemoveTagDialog()
        {
            var dialog = new RemoveTagDialog();

            if (dialog.ShowDialog() == true)
            {
                var deleted = dialog.RemovedTagIds;

                if (deleted.Any())
                {
                    foreach (var item in deleted)
                    {
                        Console.WriteLine($"🗑️ Tag deleted: {item}");
                        _tagService.DeleteTag(item);
                    }

                    // ========== RELOAD DATA AFTER DELETE ==========
                    _tagService.SaveTags();
                    LoadData(); // ✅ Reload tất cả data
                    UpdateStatistics(); // ✅ Update statistics
                    acOnTagChanged?.Invoke(); // ✅ Notify listeners

                    Console.WriteLine($"✅ Deleted {deleted.Count} tags, UI refreshed");
                }
            }
        }
        [RelayCommand]
        private void FilterByTag(Tag tag)
        {
            Console.WriteLine($"🏷️ FilterByTag: {tag.Name} (ID: {tag.Id})");

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
            Console.WriteLine($"🔄 ClearFilters");

            SelectedTag = null;
            SelectedLetter = "ALL";
            SelectedPartOfSpeech = null;

            // Reset alphabet buttons
            foreach (var item in AlphabetItems)
            {
                item.IsActive = item.Letter == "ALL";
            }

            CurrFilter = "All";
            ApplyFilters();
        }


        [RelayCommand]
        private void CreateTag()
        {
            TagPickerDialog dialog = new TagPickerDialog();
            dialog.ShowDialog();  
            LoadData();
            acOnTagChanged?.Invoke();
        }

        [RelayCommand]
        private void AddWord()
        {
            // TODO: Show dialog to add new word
            MessageBox.Show("The add new word feature is under development");
        }

        [RelayCommand]
        private void DeleteWord(string word)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete this word '{word}'?",
                "Confirmation",
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