using BlueBerryDictionary.Models;
using BlueBerryDictionary.Services;
using BlueBerryDictionary.Views.UserControls;
using System.Windows.Controls;

namespace BlueBerryDictionary.Views.Pages
{
    /// <summary>
    /// Interaction logic for FavouriteWordsPage.xaml
    /// </summary>
    public partial class FavouriteWordsPage : WordListPageBase
    {
        public FavouriteWordsPage(Action<string> onClicked) : base(onClicked)
        {
            InitializeComponent();
            LoadData();
        }
        public void LoadData() 
        {
            LoadDefCards(); 
        }
        public void LoadDefCards() 
        {
            var words = TagService.Instance.GetAllWords().Where(ws => ws.isFavorited == true);
            base.LoadDefCards(mainContent, words); 
        }


    }
}
