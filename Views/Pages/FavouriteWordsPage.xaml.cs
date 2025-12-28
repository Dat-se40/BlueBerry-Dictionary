using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using BlueBerryDictionary.Views.UserControls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace BlueBerryDictionary.Views.Pages
{
    /// <summary>
    /// Interaction logic for FavouriteWordsPage.xaml
    /// </summary>
    public partial class FavouriteWordsPage : WordListPageBase
    {
        private List<WordShortened> currentFilterWords;

        public List<WordShortened> CurrentFilterWords
        {
            get { return currentFilterWords; }
            set {
                currentFilterWords = value; 
                LoadDefCards(); 
            }
        }

        public FavouriteWordsPage(Action<string> onClicked) : base(onClicked)
        {
            InitializeComponent();
        }
        public override void LoadData()
        {
            CurrentFilterWords = fullWords;
        }
        public void LoadDefCards()
        { 
            base.LoadDefCards(mainContent, CurrentFilterWords);
        }
        public void Reload(object sender, RoutedEventArgs e) => LoadData();
        public void FilterByPartOfSpeed(object sender, RoutedEventArgs e) 
        {   
            if (CurrentFilterWords != null && sender is Button btn && btn.Tag != null)
            {
                CurrentFilterWords = fullWords.Where(ws => ws.PartOfSpeech == btn.Tag.ToString()).ToList();
            }else 
            {
                Console.WriteLine("[FavoritePage!]");
            }
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            fullWords.ForEach(fw => fw.isFavorited = false);
            CurrentFilterWords.Clear();
            mainContent.Children.Clear(); 
        }
        private List<WordShortened> fullWords => TagService.Instance.GetAllWords().Where(ws => ws.isFavorited == true).ToList();
    }
}
