using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class RateAppDialog : Window
    {
        private int _selectedRating = 0;
        private const string GITHUB_REPO = "https://github.com/Dat-se40/BlueBerry-Dictionary";
        private const string EMAIL = "24520280@gm.uit.edu.vn";

        public RateAppDialog()
        {
            InitializeComponent();
            ApplyGlobalFont();
        }

        private void GitHubStar_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show($"Không thể mở GitHub:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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