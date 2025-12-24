using System.Windows;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Dialogs.Introduces
{
    public partial class TermsDialog : Window
    {
        public TermsDialog()
        {
            InitializeComponent();
            ApplyGlobalFont();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
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