using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class ContactDialog : Window
    {
        private const string EMAIL = "24520280@gm.uit.edu.vn";
        private const string GITHUB_REPO = "https://github.com/Dat-se40/BlueBerry-Dictionary";

        public ContactDialog()
        {
            InitializeComponent();
            ApplyGlobalFont();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EmailLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenEmailClient();
        }

        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            OpenEmailClient();
        }

        private void GitHubLink_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = GITHUB_REPO,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open link:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenEmailClient()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{EMAIL}?subject=BlueBerry Dictionary - Support",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open email client:\n{ex.Message}\n\nPlease send an email manually to: {EMAIL}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Apply font từ App.Current.Resources
        /// </summary>
        private void ApplyGlobalFont()
        {
            try
            {
                if (Application.Current.Resources.Contains("AppFontFamily"))
                {
                    this.FontFamily = (System.Windows.Media.FontFamily)Application.Current.Resources["AppFontFamily"];
                }

                if (Application.Current.Resources.Contains("AppFontSize"))
                {
                    this.FontSize = (double)Application.Current.Resources["AppFontSize"];
                }

                System.Diagnostics.Debug.WriteLine($"✅ Applied font to {this.GetType().Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font to dialog error: {ex.Message}");
            }
        }

    }
}