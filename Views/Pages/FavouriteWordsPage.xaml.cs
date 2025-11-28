using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using BlueBerryDictionary.Views.UserControls;
using System.Windows.Controls;

namespace BlueBerryDictionary.Views.Pages
{
    /// <summary>
    /// Interaction logic for FavouriteWordsPage.xaml
    /// </summary>
    public partial class FavouriteWordsPage : Page
    {
        public FavouriteWordsPage()
        {
            InitializeComponent();
            LoadData();
        }
        public void LoadData() 
        {
            mainContent.Children.Clear();   
            var words = TagService.Instance.GetAllWords().Where(ws => ws.isFavorited == true);
            foreach (WordShortened word in words) 
            {
                mainContent.Children.Add(new WordDefinitionCard(word));
            }
        }
    }
}
