using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class ContactDialog : Window
    {
        private const string EMAIL = "24520280@gm.uit.edu.vn";
        private const string GITHUB_REPO = "https://github.com/Dat-se40/BlueBerry-Dictionary";

        public ContactDialog()
        {
            InitializeComponent();
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
                MessageBox.Show($"Không thể mở link:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenEmailClient()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{EMAIL}?subject=BlueBerry Dictionary - Hỗ trợ",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở email client:\n{ex.Message}\n\nVui lòng gửi email thủ công đến: {EMAIL}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}