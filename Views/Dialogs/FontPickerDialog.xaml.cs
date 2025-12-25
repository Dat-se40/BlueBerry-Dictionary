using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Dialogs
{
    public partial class FontPickerDialog : Window
    {
        public FontFamily SelectedFont { get; private set; }
        public double SelectedFontSize { get; private set; } = 14;

        public FontPickerDialog()
        {
            InitializeComponent();
            LoadSystemFonts();
            ApplyGlobalFont();
        }

        /// <summary>
        /// Load tất cả font từ hệ thống
        /// </summary>
        private void LoadSystemFonts()
        {
            var fonts = Fonts.SystemFontFamilies
                .OrderBy(f => f.Source)
                .ToList();

            FontListBox.ItemsSource = fonts;

            // Chọn Segoe UI mặc định (hoặc font đầu tiên)
            var defaultFont = fonts.FirstOrDefault(f => f.Source == "Segoe UI") ?? fonts.FirstOrDefault();
            if (defaultFont != null)
            {
                FontListBox.SelectedItem = defaultFont;
                SelectedFont = defaultFont;
            }
        }

        /// <summary>
        /// Khi user chọn font trong list
        /// </summary>
        private void FontListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontListBox.SelectedItem is FontFamily selectedFont)
            {
                SelectedFont = selectedFont;
                UpdatePreview();
            }
        }

        /// <summary>
        /// Khi user chọn font size
        /// </summary>
        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem is ComboBoxItem item &&
                double.TryParse(item.Tag?.ToString(), out double size))
            {
                SelectedFontSize = size;
                UpdatePreview();
            }
        }

        /// <summary>
        /// Cập nhật preview
        /// </summary>
        private void UpdatePreview()
        {
            if (SelectedFont != null && PreviewText != null)
            {
                PreviewText.FontFamily = SelectedFont;
                PreviewText.FontSize = SelectedFontSize;
            }
        }

        /// <summary>
        /// Áp dụng font đã chọn
        /// </summary>
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFont != null)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a font!", "Notification",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Đóng dialog
        /// </summary>
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
