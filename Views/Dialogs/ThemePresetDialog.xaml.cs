using System.Windows;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Dialogs
{
    public partial class ThemePresetDialog : Window
    {
        public string SelectedTheme { get; private set; }

        public ThemePresetDialog()
        {
            InitializeComponent();
            ApplyGlobalFont();
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button)
            {
                SelectedTheme = button.Tag?.ToString();
                DialogResult = true;
                Close();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Thêm font chữ
        /// </summary>
        private void ApplyGlobalFont()
        {
            try
            {
                if (Application.Current.Resources.Contains("AppFontFamily"))
                {
                    this.FontFamily = (FontFamily)Application.Current.Resources["AppFontFamily"];
                }

                if (Application.Current.Resources.Contains("AppFontSize"))
                {
                    this.FontSize = (double)Application.Current.Resources["AppFontSize"];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font to dialog error: {ex.Message}");
            }
        }
    }
}
