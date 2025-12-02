using BlueBerryDictionary.ViewModels;
using System.Windows.Controls;

namespace BlueBerryDictionary.Views.Pages
{
    public partial class UserProfilePage : Page
    {
        public UserProfilePage()
        {
            InitializeComponent();
            DataContext = new UserProfileViewModel(); // Bind tới ViewModel
        }
    }

}