using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class AboutDialog : Window
    {
        private const string GITHUB_REPO = "https://github.com/Dat-se40/BlueBerry-Dictionary";
        private const string EMAIL = "24520280@gm.uit.edu.vn";
        private const string WIKI = "https://github.com/Dat-se40/BlueBerry-Dictionary/wiki";

        public AboutDialog()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GitHubLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl(GITHUB_REPO);
        }

        private void EmailLink_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{EMAIL}?subject=BlueBerry Dictionary - Feedback",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open email client:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WikiLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl(WIKI);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open link:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}