using BlueBerryDictionary.Data;
using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using MyDictionary.Services;
using System.IO;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace BlueBerryDictionary.Views.UserControls
{
    public partial class WordDefinitionCard : UserControl
    {
        // Dependency Properties
        public static readonly DependencyProperty WordProperty =
            DependencyProperty.Register("Word", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PronunciationProperty =
            DependencyProperty.Register("Pronunciation", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PartOfSpeechProperty =
            DependencyProperty.Register("PartOfSpeech", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefinitionProperty =
            DependencyProperty.Register("Definition", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TimeStampProperty =
            DependencyProperty.Register("TimeStamp", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ViewCountProperty =
            DependencyProperty.Register("ViewCount", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty RegionProperty =
               DependencyProperty.Register("Region", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));
        // Properties
        public string Region
        {
            get => (string)GetValue(RegionProperty);
            set => SetValue(RegionProperty, value);
        }

        public static readonly DependencyProperty Example1Property =
            DependencyProperty.Register("Example1", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public string Example1
        {
            get => (string)GetValue(Example1Property);
            set => SetValue(Example1Property, value);
        }

        public static readonly DependencyProperty Example2Property =
            DependencyProperty.Register("Example2", typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public string Example2
        {
            get => (string)GetValue(Example2Property);
            set => SetValue(Example2Property, value);
        }

        
        public string Word
        {
            get { return (string)GetValue(WordProperty); }
            set { SetValue(WordProperty, value); }
        }

        public string Pronunciation
        {
            get { return (string)GetValue(PronunciationProperty); }
            set { SetValue(PronunciationProperty, value); }
        }

        public string PartOfSpeech
        {
            get { return (string)GetValue(PartOfSpeechProperty); }
            set { SetValue(PartOfSpeechProperty, value); }
        }

        public string Definition
        {
            get { return (string)GetValue(DefinitionProperty); }
            set { SetValue(DefinitionProperty, value); }
        }

        public string TimeStamp
        {
            get { return (string)GetValue(TimeStampProperty); }
            set { SetValue(TimeStampProperty, value); }
        }

        public string ViewCount
        {
            get { return (string)GetValue(ViewCountProperty); }
            set { SetValue(ViewCountProperty, value); }
        }
        private bool isFavorite = false ;

        public bool IsFavorite
        {
            get { return isFavorite; }
            set 
            { 
                isFavorite = value;
                OnIsFavoritedChanged(); 
            }
        }
        public WordShortened _mainWord; 
        // Events
        public event EventHandler FavoriteClicked;
        public event EventHandler DeleteClicked;
        public event EventHandler CardClicked;

        public WordDefinitionCard(Word mainWord = null)
        {
            InitializeComponent();
            DataContext = this;

            // Handle card click
            MouseLeftButtonDown += (s, e) =>
            {
                if (_mainWord != null)
                {
                    UpdateViewCount();
                }
                CardClicked?.Invoke(this, EventArgs.Empty);
            };

            if (mainWord != null) 
            {
                this.Word = mainWord.word;
                this.Pronunciation = mainWord.phonetic;
                this.Region = "US"; // Sẽ fix cái nì
                this.PartOfSpeech = mainWord.meanings[0].partOfSpeech;
                this.Definition = mainWord.meanings[0].definitions[0].definition;
                this._mainWord = WordShortened.FromWord(mainWord);  
            }
            HandleExampleAndNote();
        }
        public WordDefinitionCard(WordShortened mainWord) 
        {
            InitializeComponent();
            DataContext = this;

            MouseLeftButtonDown += (s, e) =>
            {
                if (_mainWord != null)
                {
                    UpdateViewCount();
                }
                CardClicked?.Invoke(this, EventArgs.Empty);
            };

            if (mainWord != null)
            {
                this.Word = mainWord.Word;
                this.Pronunciation = mainWord.Phonetic;
                this.Region = "UK";                 
                this.PartOfSpeech = mainWord.PartOfSpeech;
                this.Definition = mainWord.Definition;
                this.Example1 = mainWord.Example;
                this.TimeStamp = mainWord.AddedAt.ToShortDateString();
                this.IsFavorite = mainWord.isFavorited; 
                this._mainWord = mainWord;
            }
            HandleExampleAndNote(); 
        }
        public WordDefinitionCard() 
        {
            InitializeComponent();
        }
        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // Prevent card click event
            IsFavorite = !IsFavorite ;
            if (TagService.Instance.FindWordInsensitive(this.Word) is WordShortened ws) 
            {
                ws.isFavorited = IsFavorite ;
                TagService.Instance.SaveWords(); 
            }else 
            {
                TagService.Instance.AddNewWordShortened(_mainWord); 
                _mainWord.isFavorited = IsFavorite;
            }
            FavoriteClicked?.Invoke(this, EventArgs.Empty);
        }
        /*
         Note: Gán trực tiếp UI > style, resource 
                Nên phải ClearValue trước

         */
        /// <summary>
        /// Phương thức xóa, sẽ gọi các hàm dã đăng kí DeleteWord
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public Action DeleteWord; 
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // Prevent card click event
            this.Visibility = Visibility.Collapsed; 
            TagService.Instance.DeleteWordShortened(this.Word);
            File.Delete(FileStorage.GetWordFilePath(this.Word));
            DeleteWord?.Invoke(); 
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }
        private void OnIsFavoritedChanged() 
        {
            if (isFavorite == true)
            {
                btnFav.Background = Brushes.LightPink;
                btnFav.Foreground = Brushes.DeepPink; 
            }else
            {
                btnFav.ClearValue(Button.BackgroundProperty);
                btnFav.ClearValue(Button.ForegroundProperty);
                btnFav.SetResourceReference(Button.StyleProperty, "CardActionButton");
            }

        }
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(FileStorage.GetWordFilePath(Word)))
            {
                MessageBox.Show($"{{Word}} has been downloaded!");
            }
            else
            {
                var downloadedWord = await FileStorage.LoadWordAsync(Word);
                try
                {
                    FileStorage.Download(downloadedWord);
                    MessageBox.Show($"{{Word}} was downloaded successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message); 
                }

            }
        }
        private void HandleExampleAndNote()
        {
            // ========== _mainWord đã là WordShortened (rút gọn từ meaning được chọn) ==========
            Action CollapsedExample2ContainerVisility = () =>
            {
                Example2Container.Visibility = Visibility.Collapsed;    
            }; 
            if (_mainWord == null)
            {
                Example1 = string.Empty;
                Example2 = string.Empty;
                CollapsedExample2ContainerVisility?.Invoke(); 
                return;
            }

            // ========== SCENARIO 1: Có Example từ definition ==========
            if (!string.IsNullOrEmpty(_mainWord.Example))
            {
                Example1 = _mainWord.Example;

                // Nếu có note (do manual add), hiển thị ở Example2
                if (!string.IsNullOrEmpty(_mainWord.note))
                {
                    Example2 = _mainWord.note;

                    // Show Example2 Container
                    var example2Container = FindName("Example2Container") as Border;
                    if (example2Container != null)
                    {
                        example2Container.Visibility = Visibility.Visible;

                        // Change label to "Note:"
                        var example2Label = FindName("Example2Label") as TextBlock;
                        if (example2Label != null)
                            example2Label.Text = "📝 Note:";
                    }
                }
                else
                {
                    Example2 = string.Empty;
                    CollapsedExample2ContainerVisility?.Invoke();
                }
            }
            // ========== SCENARIO 2: Không có Example, nhưng có note ==========
            else if (!string.IsNullOrEmpty(_mainWord.note))
            {
                Example1 = _mainWord.note;
                Example2 = string.Empty;

                // Change Example1Label to "Note:"
                var example1Label = FindName("Example1Label") as TextBlock;
                if (example1Label != null)
                    example1Label.Text = "📝 Note:";

                CollapsedExample2ContainerVisility?.Invoke();
            }
            // ========== SCENARIO 3: Không có gì cả ==========
            else
            {
                Example1 = string.Empty;
                Example2 = string.Empty;

                // Change Example1Label
                var example1Label = FindName("Example1Label") as TextBlock;
                if (example1Label != null)
                    example1Label.Text = "Note:";

                CollapsedExample2ContainerVisility?.Invoke();
            }
        }

        private void UpdateViewCount()
        {
            if (_mainWord == null)
                return;

            _mainWord.ViewCount++;
            ViewCount = $"{_mainWord.ViewCount} views";
        }
    }
}