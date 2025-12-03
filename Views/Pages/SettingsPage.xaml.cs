using BlueBerryDictionary.Views.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace BlueBerryDictionary.Views.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        // Xử lý ComboBox Color Theme
        private void ColorThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorThemeComboBox.SelectedItem is ComboBoxItem item)
            {
                string tag = item.Tag?.ToString();

                if (tag == "preset")
                {
                    var dialog = new ThemePresetDialog { Owner = Window.GetWindow(this) };
                    if (dialog.ShowDialog() == true)
                    {
                        // Áp dụng theme (sẽ implement sau)
                        MessageBox.Show($"Đã chọn theme: {dialog.SelectedTheme}");
                    }
                    ColorThemeComboBox.SelectedIndex = 0; // Reset
                }
                else if (tag == "custom")
                {
                    var dialog = new CustomThemeDialog { Owner = Window.GetWindow(this) };
                    if (dialog.ShowDialog() == true)
                    {
                        // Áp dụng custom colors (sẽ implement sau)
                        MessageBox.Show($"Primary: {dialog.PrimaryColor}\n" +
                                        $"Secondary: {dialog.SecondaryColor}\n" +
                                        $"Accent: {dialog.AccentColor}");
                    }
                    ColorThemeComboBox.SelectedIndex = 0;
                }
            }
        }

        // Xử lý ComboBox Background Mode
        private void BackgroundModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackgroundModeComboBox.SelectedItem is ComboBoxItem item)
            {
                string tag = item.Tag?.ToString();

                if (tag == "upload")
                {
                    var dialog = new BackgroundUploadDialog { Owner = Window.GetWindow(this) };
                    if (dialog.ShowDialog() == true)
                    {
                        // Áp dụng background (sẽ implement sau)
                        MessageBox.Show($"Đã chọn ảnh: {dialog.SelectedImagePath}");
                    }
                    BackgroundModeComboBox.SelectedIndex = 0;
                }
            }
        }
    }
}