using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDictionary.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace BlueBerryDictionary.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for PackageDetailsDialog.xaml
    /// </summary>
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

        public string PackageName => _package.Name;
        public string PackageDescription => _package.Description;
        public int TotalWordsCount => _package.Container.Sum(t => t.Words.Count);

        public string SelectionSummary
        {
            get
            {
                var selected = FilteredWords?.Count(w => w.IsSelected) ?? 0;
                return $"Đã chọn: {selected}/{FilteredWords?.Count ?? 0} từ";
            }
        }

        public string EstimatedSize
        {
            get
            {
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
                .Select(w => new WordItemViewModel(w))
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
                    .Select(w => new WordItemViewModel(w))
                    .ToList();

                FilteredWords = new ObservableCollection<WordItemViewModel>(filtered);
            }

            OnPropertyChanged(nameof(SelectionSummary));
            OnPropertyChanged(nameof(EstimatedSize));
        }

        partial void OnSelectAllChanged(bool value)
        {
            foreach (var word in FilteredWords)
            {
                word.IsSelected = value;
            }
            OnPropertyChanged(nameof(SelectionSummary));
            OnPropertyChanged(nameof(EstimatedSize));
        }

        [RelayCommand]
        private async Task DownloadSelectedAsync()
        {
            var selectedWords = FilteredWords.Where(w => w.IsSelected).ToList();

            if (selectedWords.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 từ!", "Thông báo");
                return;
            }

            var result = MessageBox.Show(
                $"Tải xuống {selectedWords.Count} từ?\n\nDung lượng: {EstimatedSize}",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes) return;

            try
            {
                foreach (var wordVM in selectedWords)
                {
                    // Lưu full Word vào offline storage
                    Data.FileStorage.LoadWordAsync(new List<Word> { wordVM.Word });
                    // Sẽ thêm phương thức tạo tag
                    //// Thêm WordShortened vào TagService
                    //var shortened = WordShortened.FromWord(wordVM.Word);
                    //if (shortened != null)
                    //{
                    //    TagService.Instance.AddNewWordShortened(shortened);
                    //}
                }

               // TagService.Instance.SaveWords();

                MessageBox.Show(
                    $"✅ Đã tải xuống {selectedWords.Count} từ thành công!",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                _owner.DialogResult = true;
                _owner.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Search()
        {
            // Search logic already handled in OnSearchTextChanged
        }
    }

    public partial class WordItemViewModel : ObservableObject
    {
        public Word Word { get; }

        [ObservableProperty]
        private bool _isSelected;

        public WordItemViewModel(Word word)
        {
            Word = word;
        }

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
