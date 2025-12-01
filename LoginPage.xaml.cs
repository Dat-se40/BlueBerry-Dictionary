using BlueBerryDictionary.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BlueBerryDictionary
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel(); // Bind tới ViewModel
            LogoImg.Source = new BitmapImage(new Uri(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            @"..\..\..\Resources\Image\logo.png"
            )));

        }
    }
}