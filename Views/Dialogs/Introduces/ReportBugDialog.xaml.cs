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
                MessageBox.Show($"Cannot open GitHub:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendBugEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string subject = "BlueBerry Dictionary - Bug Report";
                string body = 
                    "Error description:%0D%0A%0D%0A" +
                    "Steps to reproduce:%0D%0A1. %0D%0A2. %0D%0A3. %0D%0A%0D%0A" +
                    "System:%0D%0AWindows: %0D%0AApp version: 1.0.0";

                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{EMAIL}?subject={subject}&body={body}",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Cannot open the email client:\n{ex.Message}\n\nPlease send the email manually to: {EMAIL}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }
    }
}