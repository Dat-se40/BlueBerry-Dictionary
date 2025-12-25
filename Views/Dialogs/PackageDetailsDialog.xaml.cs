using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDictionary.Services;
using System.Collections.ObjectModel;
using System.Windows;
using BlueBerryDictionary.Data;
using System.IO;
namespace BlueBerryDictionary.Views.Dialogs
{
    public partial class PackageDetailsDialog : Window
    {
        public PackageDetailsDialog(TopicPackage package)
        {
            InitializeComponent();
            DataContext = new PackageDetailsViewModel(package, this);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public partial class PackageDetailsViewModel : ObservableObject
    {
        private readonly TopicPackage _package;
        private readonly WordSearchService _searchService;
        private readonly Window _owner;

        [ObservableProperty]
        private ObservableCollection<WordItemViewModel> _filteredWords;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private bool _selectAll;

        // Mode: true = đã tải (quản lý offline), false = chưa tải (preview)
        public bool IsDownloadedMode => _package.IsDownloaded;

        // Hiện/ẩn checkbox + panel chọn
        public bool ShowSelectionControls => IsDownloadedMode;

        public string PackageName => _package.Name;
        public string PackageDescription => _package.Description;
        public int TotalWordsCount => _package.Container.Sum(t => t.Words.Count);

        // Text nút tải full theo mode
        public string FullDownloadButtonText =>
            IsDownloadedMode ? "💾 Update / Sync" : "💾 Download Selected";

        public string SelectionSummary
        {
            get
            {
                if (!ShowSelectionControls)
                    return string.Empty;

                var selected = FilteredWords?.Count(w => w.IsSelected) ?? 0;
                return $"Selected: {selected}/{FilteredWords?.Count ?? 0} words";
            }
        }

        public string EstimatedSize
        {
            get
            {
                if (!ShowSelectionControls)
                    return string.Empty;

                var selected = FilteredWords?.Count(w => w.IsSelected) ?? 0;
                var avgSize = 10_000; // ~10KB per word
                var totalBytes = selected * avgSize;
                return totalBytes >= 1_000_000
                    ? $"~{totalBytes / 1_000_000.0:F1} MB"
                    : $"~{totalBytes / 1_000.0:F0} KB";
            }
        }

        public PackageDetailsViewModel(TopicPackage package, Window owner)
        {
            _package = package;
            _owner = owner;
            _searchService = new WordSearchService();

            LoadWords();
        }

        private void LoadWords()
        {
            var allWords = _package.Container
                .SelectMany(topic => topic.Words)
                .Select(w => new WordItemViewModel(w, IsDownloadedMode))
                .ToList();

            FilteredWords = new ObservableCollection<WordItemViewModel>(allWords);
        }

        partial void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                LoadWords();
            }
            else
            {
                var filtered = _package.Container
                    .SelectMany(topic => topic.Words)
                    .Where(w => w.word.Contains(value, StringComparison.OrdinalIgnoreCase))
                    .Select(w => new WordItemViewModel(w, IsDownloadedMode))
                    .ToList();

                FilteredWords = new ObservableCollection<WordItemViewModel>(filtered);
            }

            OnPropertyChanged(nameof(SelectionSummary));
            OnPropertyChanged(nameof(EstimatedSize));
        }

        partial void OnSelectAllChanged(bool value)
        {
            if (!ShowSelectionControls || FilteredWords == null)
                return;

            foreach (var word in FilteredWords.Where(w => w.IsSelectable))
            {
                word.IsSelected = value;
            }
            OnPropertyChanged(nameof(SelectionSummary));
            OnPropertyChanged(nameof(EstimatedSize));
        }

        [RelayCommand]
        private async Task DownloadSelectedAsync()
        {
            if (!ShowSelectionControls)
            {
                // Mode preview: sau này bạn có thể xử lý kiểu "Tải metadata" ở đây nếu muốn
                MessageBox.Show("This is a preview mode, downloading is not supported yet.", "Notification");
                return;
            }

            var selectedWords = FilteredWords.Where(w => w.IsSelected && w.IsSelectable).ToList();

            if (selectedWords.Count == 0)
            {
                MessageBox.Show("Please select at least one word!", "Notification");
                return;
            }

            var result = MessageBox.Show(
                $"Download {selectedWords.Count} words?\n\nSize: {EstimatedSize}",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes) return;

            try
            {
                foreach (var wordVM in selectedWords)
                {
                    Data.FileStorage.LoadWordAsync(new List<Word> { wordVM.Word });
                    // chỗ này bạn có thể sync với TagService nếu muốn
                }

                MessageBox.Show(
                    $"✅ Successfully downloaded {selectedWords.Count} words!",
                    "Completed successfully",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                _owner.DialogResult = true;
                _owner.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Search()
        {
            // Đã xử lý trong OnSearchTextChanged
        }
    }

    public partial class WordItemViewModel : ObservableObject
    {
        public Word Word { get; }

        // true nếu user có thể tick (tức là trong mode đã tải, và từ này chưa có local chẳng hạn)
        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isSelectable;

        public WordItemViewModel(Word word, bool isDownloadedMode)
        {
            Word = word;

            // nếu chưa tải: không cho tick
            if (!isDownloadedMode)
            {
                IsSelectable = false;
            }
            else
            {
               
               IsSelectable = !File.Exists(FileStorage.GetWordFilePath(this.word));
            }
        }

        public string word => Word.word;
        public string phonetic => Word.phonetic;
        public string ShortDefinition
        {
            get
            {
                var firstDef = Word.meanings?.FirstOrDefault()?.definitions?.FirstOrDefault();
                return firstDef?.definition ?? "No definition";
            }
        }
    }
}
