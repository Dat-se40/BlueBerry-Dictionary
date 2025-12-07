using BlueBerryDictionary.Views.Dialogs;
using System.Windows;
using System.Windows.Controls;
using BlueBerryDictionary.ViewModels;
using BlueBerryDictionary.Services;
using System.Windows.Media;

namespace BlueBerryDictionary.Views.Pages
{
    public partial class SettingsPage : Page
    {
        private readonly SettingsViewModel _viewModel;
        private bool _isInitializing = true;
        private bool _isResetting = false;

        public SettingsPage()
        {
            InitializeComponent();
            _viewModel = new SettingsViewModel();
            DataContext = _viewModel;

            LoadCurrentSettings();

            FontFamilyComboBox.SelectionChanged += FontFamilyComboBox_SelectionChanged;
            ColorThemeComboBox.SelectionChanged += ColorThemeComboBox_SelectionChanged;
            ThemeModeComboBox.SelectionChanged += ThemeModeComboBox_SelectionChanged;
            FavouriteLimitComboBox.SelectionChanged += FavouriteLimitComboBox_SelectionChanged;

            _isInitializing = false;
            System.Diagnostics.Debug.WriteLine("✅ SettingsPage initialized");
        }

        private void LoadCurrentSettings()
        {
            var settings = SettingsService.Instance.CurrentSettings;

            // THEME MODE
            ThemeModeComboBox.SelectedIndex = settings.ThemeMode switch
            {
                "Light" => 0,
                "Dark" => 1,
                _ => 0
            };

            // ✅ COLOR THEME - XỬ LÝ 3 TRƯỜNG HỢP
            if (settings.ColorTheme == "custom" && settings.CustomColorTheme != null)
            {
                // Custom color
                ShowActiveItem(ColorThemeComboBox, 2); // Index 2: "✓ Màu tùy chỉnh"
                ColorThemeComboBox.SelectedIndex = 2;
            }
            else if (!string.IsNullOrEmpty(settings.ColorTheme) &&
                     settings.ColorTheme != "default" &&
                     settings.ColorTheme.StartsWith("theme"))
            {
                // Preset theme (theme1, theme2, ...)
                ShowActiveItem(ColorThemeComboBox, 1); // Index 1: "✓ Theme có sẵn"
                ColorThemeComboBox.SelectedIndex = 1;
            }
            else
            {
                // Default
                HideAllActiveItems(ColorThemeComboBox);
                ColorThemeComboBox.SelectedIndex = 0;
            }

            // FONT
            if (!string.IsNullOrEmpty(settings.FontFamily) && settings.FontFamily != "Segoe UI")
            {
                ShowActiveItem(FontFamilyComboBox, 1);
                FontFamilyComboBox.SelectedIndex = 1;
            }
            else
            {
                HideAllActiveItems(FontFamilyComboBox);
                FontFamilyComboBox.SelectedIndex = 0;
            }

            // FAVOURITE LIMIT
            FavouriteLimitComboBox.SelectedIndex = settings.FavouriteLimit switch
            {
                500 => 0,
                1000 => 1,
                5000 => 2,
                _ => 3
            };

            System.Diagnostics.Debug.WriteLine($"✅ Loaded: Theme={settings.ColorTheme}, Font={settings.FontFamily}");
        }

        // ========== COLOR THEME ==========
        private void ColorThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || _isResetting) return;

            if (ColorThemeComboBox.SelectedItem is ComboBoxItem item)
            {
                string tag = item.Tag?.ToString();

                // ✅ Đang ở Preset Active
                if (tag == "preset_active")
                {
                    System.Diagnostics.Debug.WriteLine("✅ Already using preset theme");
                    return;
                }

                // ✅ Đang ở Custom Active
                if (tag == "custom_active")
                {
                    System.Diagnostics.Debug.WriteLine("✅ Already using custom theme");
                    return;
                }

                // ✅ Chọn Preset Theme
                if (tag == "preset_picker")
                {
                    var dialog = new ThemePresetDialog { Owner = Window.GetWindow(this) };
                    if (dialog.ShowDialog() == true)
                    {
                        string selectedTheme = dialog.SelectedTheme;
                        ThemeManager.Instance.ApplyColorTheme(selectedTheme);

                        var settings = SettingsService.Instance.CurrentSettings;
                        settings.ColorTheme = selectedTheme;
                        settings.CustomColorTheme = null;
                        SettingsService.Instance.SaveSettings();

                        MessageBox.Show($"Đã áp dụng theme: {selectedTheme}", "Thành công",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // ✅ Chuyển sang "✓ Theme có sẵn"
                        _isResetting = true;
                        ShowActiveItem(ColorThemeComboBox, 1);
                        ColorThemeComboBox.SelectedIndex = 1;
                        _isResetting = false;
                    }
                    else
                    {
                        _isResetting = true;
                        LoadCurrentSettings();
                        _isResetting = false;
                    }
                }

                // ✅ Chọn Custom Color
                else if (tag == "custom_picker")
                {
                    var dialog = new CustomThemeDialog { Owner = Window.GetWindow(this) };
                    if (dialog.ShowDialog() == true)
                    {
                        ThemeManager.Instance.ApplyCustomColorTheme(
                            dialog.PrimaryColor,
                            dialog.SecondaryColor,
                            dialog.AccentColor
                        );

                        var settings = SettingsService.Instance.CurrentSettings;
                        settings.ColorTheme = "custom";
                        settings.CustomColorTheme = new Models.AppColorTheme
                        {
                            Primary = dialog.PrimaryColor,
                            Secondary = dialog.SecondaryColor,
                            Accent = dialog.AccentColor
                        };
                        SettingsService.Instance.SaveSettings();

                        MessageBox.Show("Đã áp dụng màu tùy chỉnh!", "Thành công",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // ✅ Chuyển sang "✓ Màu tùy chỉnh"
                        _isResetting = true;
                        ShowActiveItem(ColorThemeComboBox, 2);
                        ColorThemeComboBox.SelectedIndex = 2;
                        _isResetting = false;
                    }
                    else
                    {
                        _isResetting = true;
                        LoadCurrentSettings();
                        _isResetting = false;
                    }
                }

                // ✅ Reset về Default
                else if (tag == "default")
                {
                    var settings = SettingsService.Instance.CurrentSettings;

                    // Chỉ hỏi nếu KHÔNG phải default
                    if (settings.ColorTheme != "default" && !string.IsNullOrEmpty(settings.ColorTheme))
                    {
                        var result = MessageBox.Show(
                            "Bạn có chắc muốn reset về màu mặc định?",
                            "Xác nhận",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question
                        );

                        if (result == MessageBoxResult.Yes)
                        {
                            ThemeManager.Instance.ResetToDefaultColors();

                            settings.ColorTheme = "default";
                            settings.CustomColorTheme = null;
                            SettingsService.Instance.SaveSettings();

                            MessageBox.Show("Đã reset về màu mặc định", "Thành công",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                            _isResetting = true;
                            HideAllActiveItems(ColorThemeComboBox);
                            _isResetting = false;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Already using default theme");
                    }
                }
            }
        }

        // ========== THEME MODE ==========
        private void ThemeModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || _isResetting) return;
            if (ThemeModeComboBox.SelectedIndex >= 0)
            {
                _viewModel.ChangeThemeModeCommand.Execute(ThemeModeComboBox.SelectedIndex);
            }
        }

        // ========== FONT ==========
        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || _isResetting) return;

            if (FontFamilyComboBox.SelectedItem is ComboBoxItem item)
            {
                string tag = item.Tag?.ToString();

                if (tag == "custom_active")
                {
                    System.Diagnostics.Debug.WriteLine("✅ Already using custom font");
                    return;
                }

                if (tag == "custom_picker")
                {
                    var dialog = new FontPickerDialog { Owner = Window.GetWindow(this) };

                    if (dialog.ShowDialog() == true)
                    {
                        var selectedFont = dialog.SelectedFont;
                        var selectedSize = dialog.SelectedFontSize;

                        SettingsService.Instance.CurrentSettings.FontFamily = selectedFont.Source;
                        SettingsService.Instance.CurrentSettings.FontSize = selectedSize;
                        SettingsService.Instance.SaveSettings();

                        Application.Current.Resources["AppFontFamily"] = selectedFont;
                        Application.Current.Resources["AppFontSize"] = selectedSize;

                        foreach (Window window in Application.Current.Windows)
                        {
                            window.FontFamily = selectedFont;
                            window.FontSize = selectedSize;

                            if (window is MainWindow mainWindow)
                            {
                                var frame = mainWindow.FindName("MainFrame") as Frame;
                                if (frame?.Content is Page page)
                                {
                                    page.FontFamily = selectedFont;
                                    page.FontSize = selectedSize;
                                }
                            }
                        }

                        MessageBox.Show($"Đã áp dụng font: {selectedFont.Source} ({selectedSize}pt)", "Thành công",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        _isResetting = true;
                        ShowActiveItem(FontFamilyComboBox, 1);
                        FontFamilyComboBox.SelectedIndex = 1;
                        _isResetting = false;
                    }
                    else
                    {
                        _isResetting = true;
                        LoadCurrentSettings();
                        _isResetting = false;
                    }
                }
                else if (tag == "default")
                {
                    var settings = SettingsService.Instance.CurrentSettings;

                    if (!string.IsNullOrEmpty(settings.FontFamily) && settings.FontFamily != "Segoe UI")
                    {
                        var result = MessageBox.Show(
                            "Bạn có chắc muốn reset về font mặc định (Segoe UI 14pt)?",
                            "Xác nhận",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question
                        );

                        if (result == MessageBoxResult.Yes)
                        {
                            settings.FontFamily = "Segoe UI";
                            settings.FontSize = 14;
                            SettingsService.Instance.SaveSettings();

                            var defaultFont = new FontFamily("Segoe UI");
                            Application.Current.Resources["AppFontFamily"] = defaultFont;
                            Application.Current.Resources["AppFontSize"] = 14.0;

                            foreach (Window window in Application.Current.Windows)
                            {
                                window.FontFamily = defaultFont;
                                window.FontSize = 14;

                                if (window is MainWindow mainWindow)
                                {
                                    var frame = mainWindow.FindName("MainFrame") as Frame;
                                    if (frame?.Content is Page page)
                                    {
                                        page.FontFamily = defaultFont;
                                        page.FontSize = 14;
                                    }
                                }
                            }

                            MessageBox.Show("Đã reset về font mặc định", "Thành công",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                            _isResetting = true;
                            HideAllActiveItems(FontFamilyComboBox);
                            _isResetting = false;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Already using default font");
                    }
                }
            }
        }

        // ========== FAVOURITE LIMIT ==========
        private void FavouriteLimitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || _isResetting) return;
            if (FavouriteLimitComboBox.SelectedIndex >= 0)
            {
                _viewModel.ChangeFavouriteLimitCommand.Execute(FavouriteLimitComboBox.SelectedIndex);
            }
        }

        // ========== TOGGLE AUTO SAVE ==========
        private void ToggleAutoSave_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.ToggleAutoSaveCommand.Execute(null);
        }

        // ===== ✅ HELPER METHODS - MỚI =====

        /// <summary>
        /// Hiện active item tại index, ẩn các item khác
        /// </summary>
        private void ShowActiveItem(ComboBox comboBox, int activeIndex)
        {
            for (int i = 1; i < comboBox.Items.Count - 2; i++) // Skip index 0 (default) và 2 actions cuối
            {
                if (comboBox.Items[i] is ComboBoxItem item)
                {
                    item.Visibility = (i == activeIndex) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Ẩn tất cả active items
        /// </summary>
        private void HideAllActiveItems(ComboBox comboBox)
        {
            for (int i = 1; i < comboBox.Items.Count - 2; i++) // Skip index 0 và 2 actions cuối
            {
                if (comboBox.Items[i] is ComboBoxItem item)
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
