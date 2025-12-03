using System.Windows;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Dialogs
{
    public partial class CustomThemeDialog : Window
    {
        public Color PrimaryColor { get; private set; }
        public Color SecondaryColor { get; private set; }
        public Color AccentColor { get; private set; }

        public CustomThemeDialog()
        {
            InitializeComponent();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            // Lấy màu từ ColorPicker (với fallback)
            PrimaryColor = PrimaryColorPicker.SelectedColor ?? Colors.Blue;
            SecondaryColor = SecondaryColorPicker.SelectedColor ?? Colors.LightBlue;
            AccentColor = AccentColorPicker.SelectedColor ?? Colors.DarkBlue;

            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
