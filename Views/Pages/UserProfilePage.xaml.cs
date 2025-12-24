using BlueBerryDictionary.Services.Network;
using BlueBerryDictionary.ViewModels;
using System.Windows.Controls;

namespace BlueBerryDictionary.Views.Pages
{
    public partial class UserProfilePage : Page
    {
        UserProfileViewModel UserProfileViewModel  = new UserProfileViewModel();    
        public UserProfilePage()
        {
            InitializeComponent();
            DataContext = UserProfileViewModel; // Bind tới ViewModel
            tbUserID.Text = CloudSyncService.Instance._appFolderId;          
        }
    }

}