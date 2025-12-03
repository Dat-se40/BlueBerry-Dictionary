using System.Windows;

namespace BlueBerryDictionary.Views.Dialogs
{
    public partial class ThemePresetDialog : Window
    {
        public string SelectedTheme { get; private set; }

        public ThemePresetDialog()
        {
            InitializeComponent();
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
    }
}
