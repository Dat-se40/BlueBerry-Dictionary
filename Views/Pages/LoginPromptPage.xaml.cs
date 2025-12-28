using BlueBerryDictionary.Services;
using System.Windows;
using System.Windows.Controls;

namespace BlueBerryDictionary.Views.Pages
{
    public partial class LoginPromptPage : Page
    {
        private readonly NavigationService _navigationService;

        public LoginPromptPage(NavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService;
        }

        /// <summary>
        /// Close button (X) clicked → Stay as Guest → Back to Home
        /// </summary>
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("✅ User clicked X → Continue as Guest → Navigate to Home");
            _navigationService.NavigateTo("Home");
        }

        /// <summary>
        /// Go to Login button clicked → Navigate to LoginWindow
        /// </summary>
        private void GoToLoginBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("✅ User clicked Go to Login → Show LoginWindow");

            // Close MainWindow
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.Hide(); // Hide instead of Close để không shutdown app

            // Show LoginWindow
            var loginWindow = new LoginWindow();
            bool? loginResult = loginWindow.ShowDialog();

            // Handle login result
            if (loginResult == true)
            {
                // User logged in successfully
                System.Diagnostics.Debug.WriteLine("✅ Login success → Navigate to UserProfile");
                mainWindow?.Show();
                _navigationService.NavigateTo("UserProfile");
            }
            else if (loginResult == false)
            {
                // User chose Guest again
                System.Diagnostics.Debug.WriteLine("✅ Guest mode again → Navigate to Home");
                mainWindow?.Show();
                _navigationService.NavigateTo("Home");
            }
            else
            {
                // User closed LoginWindow (X button)
                System.Diagnostics.Debug.WriteLine("⚠️ User cancelled login → Stay on current page");
                mainWindow?.Show();
            }
        }
    }
}
