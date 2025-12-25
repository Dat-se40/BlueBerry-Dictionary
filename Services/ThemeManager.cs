using System;
using System.Windows;
using System.Windows.Media;
using BlueBerryDictionary.Models;

namespace BlueBerryDictionary.Services
{
    public enum ThemeMode
    {
        Light,
        Dark,
        Auto
    }

    public class ThemeManager
    {
        private static ThemeManager _instance;
        private ThemeMode _currentMode = ThemeMode.Light;

        private ResourceDictionary _appResources;

        public static ThemeManager Instance => _instance ??= new ThemeManager();

        public ThemeMode CurrentTheme { get; private set; } = ThemeMode.Light;
        public string CurrentColorTheme { get; private set; } = "theme1";

        // Lưu theme object hiện tại
        private AppColorTheme _currentThemeObject;

        public event Action<ThemeMode> ThemeChanged;

        private ThemeManager()
        {
            _appResources = Application.Current.Resources;
            // Load theme mặc định
            _currentThemeObject = ThemePresets.GetTheme("theme1");
            SystemThemeDetector.SystemThemeChanged += OnSystemThemeChanged;
        }


        /// <summary>
        /// Set theme mode (Light/Dark/Auto)
        /// </summary>
        public void SetThemeMode(ThemeMode mode)
        {
            _currentMode = mode;
            CurrentTheme = mode;

            System.Diagnostics.Debug.WriteLine($"🎨 [ThemeManager] Mode changed: {mode}");

            if (mode == ThemeMode.Auto)
            {
                // Bật theo dõi system theme
                SystemThemeDetector.StartWatching();

                // Apply theme theo system hiện tại
                ApplySystemTheme();

                var settings = SettingsService.Instance.CurrentSettings;
                settings.ThemeMode = "Auto";
                SettingsService.Instance.SaveSettings();

                System.Diagnostics.Debug.WriteLine("✅ [ThemeManager] Auto mode enabled");
            }
            else
            {
                // Tắt theo dõi system theme
                SystemThemeDetector.StopWatching();

                // Determine actual theme (Light or Dark)
                ThemeMode actualMode = mode;

                // Nếu đang ở "default", phải reload Colors.xaml
                if (CurrentColorTheme == "default" || _currentThemeObject == null)
                {
                    ReloadDefaultColors(actualMode);
                }
                else
                {
                    // Re-apply custom/preset theme
                    ApplyColorTheme(_currentThemeObject);
                }

                UpdateSearchInputColor();

                // Trigger event
                ThemeChanged?.Invoke(actualMode);

                var settings = SettingsService.Instance.CurrentSettings;
                settings.ThemeMode = actualMode == ThemeMode.Light ? "Light" : "Dark";
                SettingsService.Instance.SaveSettings();

                System.Diagnostics.Debug.WriteLine($"✅ [ThemeManager] Manual mode: {actualMode} (ColorTheme: {CurrentColorTheme})");
            }
        }


        /// <summary>
        /// Apply Light or Dark theme (BASE COLORS ONLY)
        /// </summary>
        private void ApplyTheme(ThemeMode mode)
        {
            string prefix = mode == ThemeMode.Light ? "Light" : "Dark";

            // Update ALL resources
            UpdateResource("MainBackground", $"{prefix}MainBackground");
            UpdateResource("NavbarBackground", $"{prefix}NavbarBackground");
            UpdateResource("ToolbarBackground", $"{prefix}ToolbarBackground");
            UpdateResource("SidebarBackground", $"{prefix}SidebarBackground");
            UpdateResource("CardBackground", $"{prefix}CardBackground");
            UpdateResource("WordItemBackground", $"{prefix}WordItemBackground");
            UpdateResource("WordItemHover", $"{prefix}WordItemHover");
            UpdateResource("MeaningBackground", $"{prefix}MeaningBackground");
            UpdateResource("MeaningBorder", $"{prefix}MeaningBorder");
            UpdateResource("MeaningBorderLeft", $"{prefix}MeaningBorderLeft");
            UpdateResource("ExampleBackground", $"{prefix}ExampleBackground");
            UpdateResource("ExampleBorder", $"{prefix}ExampleBorder");
            UpdateResource("RelatedBackground", $"{prefix}RelatedBackground");
            UpdateResource("RelatedBorder", $"{prefix}RelatedBorder");

            UpdateResource("TextColor", $"{prefix}TextColor");
            UpdateResource("BorderColor", $"{prefix}BorderColor");
            UpdateResource("ButtonColor", $"{prefix}ButtonColor");
            UpdateResource("WordBorder", $"{prefix}WordBorder");
            UpdateResource("SearchBackground", $"{prefix}SearchBackground");
            UpdateResource("SearchBorder", $"{prefix}SearchBorder");
            UpdateResource("SearchIcon", $"{prefix}SearchIcon");
            UpdateResource("SearchText", $"{prefix}SearchText");
            UpdateResource("SearchPlaceholder", $"{prefix}SearchPlaceholder");
            UpdateResource("SearchButton", $"{prefix}SearchButton");
            UpdateResource("SearchButtonHover", $"{prefix}SearchButtonHover");
            UpdateResource("ToolButtonActive", $"{prefix}ToolButtonActive");
            UpdateResource("NavButtonColor", $"{prefix}NavButtonColor");
            UpdateResource("NavButtonHover", $"{prefix}NavButtonHover");
            UpdateResource("HamburgerBackground", $"{prefix}HamburgerBackground");
            UpdateResource("HamburgerHover", $"{prefix}HamburgerHover");
            UpdateResource("HamburgerIcon", $"{prefix}HamburgerIcon");
            UpdateResource("ThemeToggleBackground", $"{prefix}ThemeToggleBackground");
            UpdateResource("ThemeSliderBackground", $"{prefix}ThemeSliderBackground");
            UpdateResource("ThemeIconColor", $"{prefix}ThemeIconColor");
            UpdateResource("ToolbarBorder", $"{prefix}ToolbarBorder");
            UpdateResource("SidebarHover", $"{prefix}SidebarHover");
            UpdateResource("SidebarHoverText", $"{prefix}SidebarHoverText");

            // ✅ THÊM: Suggestions Popup
            UpdateResource("SuggestionsBackground", $"{prefix}SuggestionsBackground");
            UpdateResource("SuggestionsBorder", $"{prefix}SuggestionsBorder");
            UpdateResource("SuggestionsItemBorder", $"{prefix}SuggestionsItemBorder");
            UpdateResource("SuggestionsItemHover", $"{prefix}SuggestionsItemHover");
            UpdateResource("SuggestionsItemSelected", $"{prefix}SuggestionsItemSelected");
        }

        /// <summary>
        /// Apply color theme (từ ThemePresets)
        /// </summary>
        public void ApplyColorTheme(string themeName)
        {
            var theme = ThemePresets.GetTheme(themeName);
            if (theme == null) return;

            CurrentColorTheme = themeName;
            _currentThemeObject = theme; // LƯU THEME OBJECT

            ApplyColorTheme(theme);
            SettingsService.Instance.SaveColorTheme(themeName, null);
        }

        /// <summary>
        /// Apply custom color theme
        /// </summary>
        public void ApplyCustomColorTheme(Color primary, Color secondary, Color accent)
        {
            var theme = new AppColorTheme
            {
                Primary = primary,
                Secondary = secondary,
                Accent = accent
            };

            CurrentColorTheme = "custom";
            _currentThemeObject = theme; // LƯU THEME OBJECT

            ApplyColorTheme(theme);
            SettingsService.Instance.SaveColorTheme("custom", theme);
        }

        /// <summary>
        /// Apply colors from theme object
        /// </summary>
        private void ApplyColorTheme(AppColorTheme theme)
        {
            string prefix = CurrentTheme == ThemeMode.Light ? "Light" : "Dark";

            // Light Mode Colors
            if (CurrentTheme == ThemeMode.Light)
            {
                // === BACKGROUNDS (Gradients) ===
                Color lightest = LightenColor(theme.Secondary, 0.9);
                Color lighter = LightenColor(theme.Secondary, 0.8);
                Color light = LightenColor(theme.Secondary, 0.7);

                // MainBackground (3-color gradient)
                UpdateGradientBrush(_appResources, "LightMainBackground",
                    lightest, lighter, light);

                // Navbar (Accent → Primary)
                UpdateGradientBrush(_appResources, "LightNavbarBackground",
                    theme.Accent, theme.Primary);

                // Toolbar
                UpdateGradientBrush(_appResources, "LightToolbarBackground",
                    lightest, light);

                // Sidebar
                UpdateGradientBrush(_appResources, "LightSidebarBackground",
                    Color.FromRgb(255, 255, 255), lightest);

                // Card
                UpdateGradientBrush(_appResources, "LightCardBackground",
                    Color.FromRgb(255, 255, 255), lightest);

                // WordItemBackground
                UpdateGradientBrush(_appResources, "LightWordItemBackground",
                    lightest, light);

                // WordItemHover (darker)
                Color hoverColor1 = LightenColor(theme.Secondary, 0.6);
                Color hoverColor2 = LightenColor(theme.Secondary, 0.65);
                UpdateGradientBrush(_appResources, "LightWordItemHover",
                    hoverColor1, hoverColor2);

                // SidebarHover
                UpdateGradientBrush(_appResources, "LightSidebarHover",
                    lightest, light);

                // === SOLID COLORS ===
                UpdateSolidBrush(_appResources, "LightTextColor", theme.Accent);
                UpdateSolidBrush(_appResources, "LightBorderColor", theme.Secondary);
                UpdateSolidBrush(_appResources, "LightButtonColor", theme.Primary);
                UpdateSolidBrush(_appResources, "LightWordBorder", theme.Primary);
                UpdateSolidBrush(_appResources, "LightMeaningBorderLeft", theme.Accent);
                UpdateSolidBrush(_appResources, "LightExampleBorder", theme.Secondary);
                UpdateSolidBrush(_appResources, "LightThemeSliderBackground", theme.Secondary);
                UpdateSolidBrush(_appResources, "LightThemeIconColor", theme.Primary);
                UpdateSolidBrush(_appResources, "LightSidebarHoverText", theme.Accent);

                // Search Button
                UpdateGradientBrush(_appResources, "LightSearchButton",
                    theme.Primary, theme.Secondary);
                UpdateGradientBrush(_appResources, "LightSearchButtonHover",
                    theme.Accent, theme.Primary);

                // Tool Button
                UpdateGradientBrush(_appResources, "LightToolButtonActive",
                    theme.Primary, theme.Secondary);

                // ✅ THÊM: Suggestions Popup Colors (Light Mode)
                UpdateSolidBrush(_appResources, "LightSuggestionsBackground", Color.FromRgb(255, 255, 255));
                UpdateSolidBrush(_appResources, "LightSuggestionsBorder", theme.Secondary);
                UpdateSolidBrush(_appResources, "LightSuggestionsItemBorder", lightest);
                UpdateSolidBrush(_appResources, "LightSuggestionsItemHover", lightest);
                UpdateSolidBrush(_appResources, "LightSuggestionsItemSelected", hoverColor1);
            }
            // Dark Mode Colors
            else
            {
                // === BACKGROUNDS (Gradients) ===
                Color darkest = Color.FromRgb(13, 27, 42);    // #0D1B2A
                Color darker = Color.FromRgb(27, 38, 59);     // #1B263B
                Color dark = Color.FromRgb(30, 41, 59);       // #1E293B

                // MainBackground
                UpdateGradientBrush(_appResources, "DarkMainBackground",
                    darkest, darker);

                // Navbar (Darker version of theme colors)
                UpdateGradientBrush(_appResources, "DarkNavbarBackground",
                    DarkenColor(theme.Accent, 0.5), DarkenColor(theme.Primary, 0.5));

                // Toolbar
                UpdateGradientBrush(_appResources, "DarkToolbarBackground",
                    dark, Color.FromRgb(51, 65, 85)); // #334155

                // Sidebar
                UpdateGradientBrush(_appResources, "DarkSidebarBackground",
                    dark, Color.FromRgb(15, 23, 42)); // #0F172A

                // Card
                UpdateGradientBrush(_appResources, "DarkCardBackground",
                    dark, Color.FromRgb(51, 65, 85));

                // WordItemBackground (solid in dark mode)
                UpdateSolidBrush(_appResources, "DarkWordItemBackground",
                    Color.FromRgb(45, 55, 72)); // #2D3748

                // WordItemHover
                UpdateSolidBrush(_appResources, "DarkWordItemHover",
                    Color.FromRgb(55, 65, 81)); // #374151

                // SidebarHover
                UpdateSolidBrush(_appResources, "DarkSidebarHover",
                    Color.FromRgb(51, 65, 85));

                // === SOLID COLORS ===
                UpdateSolidBrush(_appResources, "DarkTextColor", theme.Secondary);
                UpdateSolidBrush(_appResources, "DarkBorderColor", theme.Primary);
                UpdateSolidBrush(_appResources, "DarkButtonColor", theme.Secondary);
                UpdateSolidBrush(_appResources, "DarkWordBorder", theme.Secondary);
                UpdateSolidBrush(_appResources, "DarkMeaningBorderLeft", theme.Primary);
                UpdateSolidBrush(_appResources, "DarkExampleBorder", theme.Secondary);
                UpdateSolidBrush(_appResources, "DarkThemeSliderBackground", theme.Primary);
                UpdateSolidBrush(_appResources, "DarkThemeIconColor", theme.Secondary);
                UpdateSolidBrush(_appResources, "DarkSidebarHoverText", theme.Secondary);

                // Search Button
                UpdateGradientBrush(_appResources, "DarkSearchButton",
                    theme.Primary, theme.Secondary);
                UpdateGradientBrush(_appResources, "DarkSearchButtonHover",
                    DarkenColor(theme.Primary, 0.2), theme.Primary);

                // Tool Button
                UpdateGradientBrush(_appResources, "DarkToolButtonActive",
                    theme.Primary, theme.Secondary);
                // ✅ THÊM: Suggestions Popup Colors (Dark Mode)
                UpdateSolidBrush(_appResources, "DarkSuggestionsBackground", dark);
                UpdateSolidBrush(_appResources, "DarkSuggestionsBorder", theme.Primary);
                UpdateSolidBrush(_appResources, "DarkSuggestionsItemBorder", Color.FromRgb(51, 65, 85));
                UpdateSolidBrush(_appResources, "DarkSuggestionsItemHover", Color.FromRgb(51, 65, 85));
                UpdateSolidBrush(_appResources, "DarkSuggestionsItemSelected", Color.FromRgb(71, 85, 105));
            }

            // Apply current theme mode
            ApplyTheme(CurrentTheme);

            System.Diagnostics.Debug.WriteLine($"✅ Applied color theme: {CurrentColorTheme} ({CurrentTheme} mode)");
        }

        #region Helper methods

        /// <summary>
        /// Lighten color (cho Light mode backgrounds)
        /// </summary>
        private Color LightenColor(Color color, double factor)
        {
            return Color.FromRgb(
                (byte)(color.R + (255 - color.R) * factor),
                (byte)(color.G + (255 - color.G) * factor),
                (byte)(color.B + (255 - color.B) * factor)
            );
        }

        /// <summary>
        /// Darken color (cho Dark mode)
        /// </summary>
        private Color DarkenColor(Color color, double factor)
        {
            return Color.FromRgb(
                (byte)(color.R * (1 - factor)),
                (byte)(color.G * (1 - factor)),
                (byte)(color.B * (1 - factor))
            );
        }

        /// <summary>
        /// Update dynamic resource
        /// </summary>
        private void UpdateResource(string key, string sourceKey)
        {
            if (_appResources.Contains(sourceKey))
            {
                _appResources[key] = _appResources[sourceKey];
            }
        }

        /// <summary>
        /// Update solid brush
        /// </summary>
        private void UpdateSolidBrush(ResourceDictionary resources, string key, Color color)
        {
            resources[key] = new SolidColorBrush(color);
        }

        /// <summary>
        /// Update gradient brush
        /// </summary>
        private void UpdateGradientBrush(ResourceDictionary resources, string key,
            Color color1, Color color2, Color? color3 = null)
        {
            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            gradient.GradientStops.Add(new GradientStop(color1, 0));
            gradient.GradientStops.Add(new GradientStop(color2, color3.HasValue ? 0.5 : 1));

            if (color3.HasValue)
            {
                gradient.GradientStops.Add(new GradientStop(color3.Value, 1));
            }

            resources[key] = gradient;
        }

        /// <summary>
        /// Reload màu mặc định từ Colors.xaml khi toggle Light/Dark
        /// </summary>
        private void ReloadDefaultColors(ThemeMode mode)
        {
            try
            {
                // reload Colors.xaml để lấy màu gốc
                var colorsDict = new ResourceDictionary
                {
                    Source = new Uri("Resources/Styles/Colors.xaml", UriKind.Relative)
                };

                string prefix = mode == ThemeMode.Light ? "Light" : "Dark";

                // Danh sách resources cần reload
                var resourcesToReset = new[]
                {
            "MainBackground", "NavbarBackground", "ToolbarBackground",
            "SidebarBackground", "CardBackground", "WordItemBackground",
            "WordItemHover", "MeaningBackground", "MeaningBorder",
            "MeaningBorderLeft", "ExampleBackground", "ExampleBorder",
            "RelatedBackground", "RelatedBorder", "TextColor",
            "BorderColor", "ButtonColor", "WordBorder",
            "SearchBackground", "SearchBorder", "SearchIcon",
            "SearchText", "SearchPlaceholder", "SearchButton",
            "SearchButtonHover", "ToolButtonActive", "NavButtonColor",
            "NavButtonHover", "HamburgerBackground", "HamburgerHover",
            "HamburgerIcon", "ThemeToggleBackground", "ThemeSliderBackground",
            "ThemeIconColor", "ToolbarBorder", "SidebarHover", "SidebarHoverText"
        };

                // Copy từ Colors.xaml mới load
                foreach (var key in resourcesToReset)
                {
                    string sourceKey = $"{prefix}{key}";

                    if (colorsDict.Contains(sourceKey))
                    {
                        _appResources[key] = colorsDict[sourceKey];
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Reloaded default colors for {mode} mode");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to reload default colors: {ex.Message}");
            }
        }


        /// <summary>
        /// Reset về màu mặc định trong Colors.xaml (Blue Gradient)
        /// </summary>
        public void ResetToDefaultColors()
        {
            try
            {
                // RELOAD Colors.xaml để lấy màu gốc (chưa bị override)
                var colorsDict = new ResourceDictionary
                {
                    Source = new Uri("Resources/Styles/Colors.xaml", UriKind.Relative)
                };

                // Xác định prefix theo theme mode hiện tại
                string prefix = CurrentTheme == ThemeMode.Dark ? "Dark" : "Light";

                // Danh sách tất cả resources cần reset
                var resourcesToReset = new[]
                {
            "MainBackground", "NavbarBackground", "ToolbarBackground",
            "SidebarBackground", "CardBackground", "WordItemBackground",
            "WordItemHover", "MeaningBackground", "MeaningBorder",
            "MeaningBorderLeft", "ExampleBackground", "ExampleBorder",
            "RelatedBackground", "RelatedBorder", "TextColor",
            "BorderColor", "ButtonColor", "WordBorder",
            "SearchBackground", "SearchBorder", "SearchIcon",
            "SearchText", "SearchPlaceholder", "SearchButton",
            "SearchButtonHover", "ToolButtonActive", "NavButtonColor",
            "NavButtonHover", "HamburgerBackground", "HamburgerHover",
            "HamburgerIcon", "ThemeToggleBackground", "ThemeSliderBackground",
            "ThemeIconColor", "ToolbarBorder", "SidebarHover", "SidebarHoverText"
        };

                // Copy từ Colors.xaml mới load (màu gốc) vào Application.Resources
                foreach (var key in resourcesToReset)
                {
                    string sourceKey = $"{prefix}{key}";

                    if (colorsDict.Contains(sourceKey))
                    {
                        _appResources[key] = colorsDict[sourceKey];
                        System.Diagnostics.Debug.WriteLine($"  ✅ Reset {key} from {sourceKey}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  ⚠️ Missing resource: {sourceKey}");
                    }
                }

                // Reset internal state
                CurrentColorTheme = "default";
                _currentThemeObject = null;

                System.Diagnostics.Debug.WriteLine($"✅ Successfully reset to default colors from Colors.xaml ({prefix} mode)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to reset colors: {ex.Message}");
                MessageBox.Show($"Lỗi reset màu:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Cập nhật màu chữ SearchInput khi toggle theme
        /// </summary>
        private void UpdateSearchInputColor()
        {
            try
            {
                // Tìm MainWindow
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow == null) return;

                // Tìm SearchInput TextBox
                var searchInput = mainWindow.FindName("SearchInput") as System.Windows.Controls.TextBox;
                if (searchInput == null) return;

                // Lấy brush màu hiện tại từ Resources
                var searchTextBrush = _appResources["SearchText"] as Brush;
                var placeholderBrush = _appResources["SearchPlaceholder"] as Brush;

                // Kiểm tra placeholder hay có chữ
                if (searchInput.Text == "Nhập từ cần tra...")
                {
                    // Placeholder → màu xám
                    searchInput.Foreground = placeholderBrush;
                }
                else if (!string.IsNullOrEmpty(searchInput.Text))
                {
                    // Có chữ → màu SearchText (đen/trắng tùy theme)
                    searchInput.Foreground = searchTextBrush;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Updated SearchInput color (Mode: {CurrentTheme})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Failed to update SearchInput color: {ex.Message}");
            }
        }

        /// <summary>
        /// Dùng theme theo system hiện tại
        /// </summary>
        private void ApplySystemTheme()
        {
            bool isDark = SystemThemeDetector.IsSystemDarkMode();
            ThemeMode actualMode = isDark ? ThemeMode.Dark : ThemeMode.Light;

            System.Diagnostics.Debug.WriteLine($"🔍 [ThemeManager] System theme detected: {(isDark ? "Dark" : "Light")}");

            // Apply theme
            CurrentTheme = actualMode;

            if (CurrentColorTheme == "default" || _currentThemeObject == null)
            {
                ReloadDefaultColors(actualMode);
            }
            else
            {
                ApplyColorTheme(_currentThemeObject);
            }

            UpdateSearchInputColor();
            ThemeChanged?.Invoke(actualMode);

            System.Diagnostics.Debug.WriteLine($"✅ [ThemeManager] Applied system theme → {actualMode}");
        }

        /// <summary>
        /// Handler khi system theme thay đổi
        /// </summary>
        private void OnSystemThemeChanged(object sender, bool isDark)
        {
            // Chỉ apply nếu đang ở Auto mode
            if (_currentMode == ThemeMode.Auto)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ThemeMode newMode = isDark ? ThemeMode.Dark : ThemeMode.Light;
                    CurrentTheme = newMode;

                    System.Diagnostics.Debug.WriteLine($"🔄 [ThemeManager] System theme changed → {(isDark ? "Dark" : "Light")}");

                    if (CurrentColorTheme == "default" || _currentThemeObject == null)
                    {
                        ReloadDefaultColors(newMode);
                    }
                    else
                    {
                        ApplyColorTheme(_currentThemeObject);
                    }

                    UpdateSearchInputColor();
                    ThemeChanged?.Invoke(newMode);

                    System.Diagnostics.Debug.WriteLine($"✅ [ThemeManager] Auto mode applied: {newMode}");
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [ThemeManager] System theme changed but Auto mode is OFF (current: {_currentMode})");
            }
        }
        #endregion
    }
}
