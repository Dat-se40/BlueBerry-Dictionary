using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class LicensesDialog : Window
    {
        public LicensesDialog()
        {
            InitializeComponent();
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
    }
}