using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class LicensesDialog : Window
    {
        public LicensesDialog()
        {
            InitializeComponent();
            ApplyGlobalFont();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AppLicense_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://github.com/Dat-se40/BlueBerry-Dictionary/blob/main/LICENSE");
        }

        private void DotNetLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://github.com/dotnet/runtime");
        }

        private void NewtonsoftLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://www.newtonsoft.com/json");
        }

        private void GoogleAPILink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://developers.google.com/drive");
        }

        private void FreeDictLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://dictionaryapi.dev");
        }

        private void MerriamLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://dictionaryapi.com");
        }

        private void CambridgeLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://dictionary.cambridge.org");
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
                MessageBox.Show($"Không thể mở link:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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