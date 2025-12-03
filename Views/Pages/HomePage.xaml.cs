namespace BlueBerryDictionary.Views.Pages
{
    using BlueBerryDictionary.Data;
    using BlueBerryDictionary.Models;
    using BlueBerryDictionary.Views.UserControls;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : WordListPageBase
    {
        /// <summary>
        /// Defines the Navigate
        /// </summary>
        public Action<object, RoutedEventArgs> Navigate;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomePage"/> class.
        /// </summary>
        /// <param name="action">The action<see cref="Action{string}"/></param>
        /// <param name="navigate">The navigate<see cref="Action{object, RoutedEventArgs}"/></param>
        /// 
        List<Quote> listQuotes;
        public HomePage(Action<string> action, Action<object, RoutedEventArgs> navigate) : base(action)
        {
            InitializeComponent();
            Navigate += navigate;
            listQuotes = new List<Quote>();
            this.Loaded += Home_Loaded1; 
        }

        /// <summary>
        /// The ButtnNavigate_Click
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/></param>
        internal void ButtnNavigate_Click(object sender, RoutedEventArgs e)
        {
            Navigate?.Invoke(sender, e);
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        public override void LoadData() 
        {
            _ = InitializeAsync();  
        }
        #region Load random qoute 
        private void Home_Loaded1(object sender, RoutedEventArgs e)
        {
            stpWordItems.Children.Clear();
            LoadRandomContent();
        }
        private async Task InitializeAsync()
        {
            await LoadAllQuotes();
        }
        void AddWordItem(Word mainWord)
        {
            WordItem wordItem = new WordItem();
            wordItem.SetUpDisplay(mainWord);
            wordItem.HorizontalAlignment = HorizontalAlignment.Stretch;
            stpWordItems.Children.Add(wordItem);
        }
        void AddWordItem(List<ShortenWord> words)
        {
            foreach (var word in words)
            {
                WordItem wordItem = new WordItem();
                wordItem.OnWordClick += base._onWordClick; 
                wordItem.SetUpDisplay(word);
                wordItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                stpWordItems.Children.Add(wordItem);
            }
        }
        private async Task LoadAllQuotes()
        {
            var quotePaths = Directory.GetFiles(FileStorage._storedQuotePath);
            foreach (var quotePath in quotePaths)
            {
                var task = FileStorage.LoadQuoteAsync(quotePath);
                Quote quote = await task;
                if (quote != null) listQuotes.Add(quote);
            }
        }
        private void LoadRandomContent()
        {
            Random ran = new Random();
            int ID = ran.Next(0, listQuotes.Count) + 1;
            LoadContent(ID);
        }
        private void LoadContent(int ID)
        {
            int index = ID - 1;
            if (index >= 0 && index < listQuotes.Count)
            {
                Quote mainQuote = listQuotes[ID - 1];
                QuoteText.Text = mainQuote.content;
                QuoteAuthor.Text = mainQuote.author;
                QuoteImage.ImageSource = new BitmapImage(new Uri(mainQuote.imageUrl));
                AddWordItem(mainQuote.relativeWords);
            }
        }
        #endregion
    }

}
