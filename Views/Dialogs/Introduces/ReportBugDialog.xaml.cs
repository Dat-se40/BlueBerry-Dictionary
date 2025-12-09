using System.Diagnostics;
using System.Windows;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class ReportBugDialog : Window
    {
        private const string EMAIL = "24520280@gm.uit.edu.vn";
        private const string GITHUB_ISSUES = "https://github.com/Dat-se40/BlueBerry-Dictionary/issues/new";

        public ReportBugDialog()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenGitHubIssues_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = GITHUB_ISSUES,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở GitHub:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendBugEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string subject = "BlueBerry Dictionary - Báo lỗi";
                string body = "Mô tả lỗi:%0D%0A%0D%0ACác bước tái hiện:%0D%0A1. %0D%0A2. %0D%0A3. %0D%0A%0D%0AHệ thống:%0D%0AWindows: %0D%0AApp version: 1.0.0";

                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{EMAIL}?subject={subject}&body={body}",
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