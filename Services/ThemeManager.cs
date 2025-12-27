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
        private ResourceDictionary _appResources;

        public static ThemeManager Instance => _instance ??= new ThemeManager();
        public ThemeMode CurrentTheme { get; private set; } = ThemeMode.Light;
        public string CurrentColorTheme { get; private set; } = "theme1";

        private AppColorTheme _currentThemeObject;
        public event Action<ThemeMode> ThemeChanged;

        private ThemeManager()
        {
            _appResources = Application.Current.Resources;
            _currentThemeObject = ThemePresets.GetTheme("theme1");
        }

        public void SetThemeMode(ThemeMode mode)
        {
            CurrentTheme = mode;
            if (mode == ThemeMode.Auto)
            {
                mode = ThemeMode.Light;
            }

            if (CurrentColorTheme == "default" || _currentThemeObject == null)
            {
                ReloadDefaultColors(mode);
            }
            else
            {
                ApplyColorTheme(_currentThemeObject);
            }

            UpdateSearchInputColor();
            ThemeChanged?.Invoke(mode);

            string themeString = mode == ThemeMode.Light ? "Light" : "Dark";
            SettingsService.Instance.CurrentSettings.ThemeMode = themeString;
            SettingsService.Instance.SaveSettings();

            System.Diagnostics.Debug.WriteLine($"✅ Theme mode changed to: {mode} (ColorTheme: {CurrentColorTheme})");
        }

        private void ApplyTheme(ThemeMode mode)
        {
            string prefix = mode == ThemeMode.Light ? "Light" : "Dark";

            // Backgrounds
            UpdateResource("MainBackground", $"{prefix}MainBackground");
            UpdateResource("NavbarBackground", $"{prefix}NavbarBackground");
            UpdateResource("ToolbarBackground", $"{prefix}ToolbarBackground");
            UpdateResource("SidebarBackground", $"{prefix}SidebarBackground");
            UpdateResource("CardBackground", $"{prefix}CardBackground");
            UpdateResource("WordItemBackground", $"{prefix}WordItemBackground");
            UpdateResource("WordItemHover", $"{prefix}WordItemHover");

            // Meaning Section
            UpdateResource("MeaningBackground", $"{prefix}MeaningBackground");
            UpdateResource("MeaningBorder", $"{prefix}MeaningBorder");
            UpdateResource("MeaningBorderLeft", $"{prefix}MeaningBorderLeft");

            // Example
            UpdateResource("ExampleBackground", $"{prefix}ExampleBackground");
            UpdateResource("ExampleBorder", $"{prefix}ExampleBorder");

            // Related
            UpdateResource("RelatedBackground", $"{prefix}RelatedBackground");
            UpdateResource("RelatedBorder", $"{prefix}RelatedBorder");

            // Text & Buttons
            UpdateResource("TextColor", $"{prefix}TextColor");
            UpdateResource("BorderColor", $"{prefix}BorderColor");
            UpdateResource("ButtonColor", $"{prefix}ButtonColor");
            UpdateResource("WordBorder", $"{prefix}WordBorder");

            // Search
            UpdateResource("SearchBackground", $"{prefix}SearchBackground");
            UpdateResource("SearchBorder", $"{prefix}SearchBorder");
            UpdateResource("SearchIcon", $"{prefix}SearchIcon");
            UpdateResource("SearchText", $"{prefix}SearchText");
            UpdateResource("SearchPlaceholder", $"{prefix}SearchPlaceholder");
            UpdateResource("SearchButton", $"{prefix}SearchButton");
            UpdateResource("SearchButtonHover", $"{prefix}SearchButtonHover");

            // ✅ THÊM: Suggestions Popup
            UpdateResource("SuggestionsBackground", $"{prefix}SuggestionsBackground");
            UpdateResource("SuggestionsBorder", $"{prefix}SuggestionsBorder");
            UpdateResource("SuggestionsItemBorder", $"{prefix}SuggestionsItemBorder");
            UpdateResource("SuggestionsItemHover", $"{prefix}SuggestionsItemHover");
            UpdateResource("SuggestionsItemSelected", $"{prefix}SuggestionsItemSelected");

            // Tool Buttons
            UpdateResource("ToolButtonActive", $"{prefix}ToolButtonActive");
            UpdateResource("NavButtonColor", $"{prefix}NavButtonColor");
            UpdateResource("NavButtonHover", $"{prefix}NavButtonHover");

            // Hamburger
            UpdateResource("HamburgerBackground", $"{prefix}HamburgerBackground");
            UpdateResource("HamburgerHover", $"{prefix}HamburgerHover");
            UpdateResource("HamburgerIcon", $"{prefix}HamburgerIcon");

            // Theme Toggle
            UpdateResource("ThemeToggleBackground", $"{prefix}ThemeToggleBackground");
            UpdateResource("ThemeSliderBackground", $"{prefix}ThemeSliderBackground");
            UpdateResource("ThemeIconColor", $"{prefix}ThemeIconColor");

            // Others
            UpdateResource("ToolbarBorder", $"{prefix}ToolbarBorder");
            UpdateResource("SidebarHover", $"{prefix}SidebarHover");
            UpdateResource("SidebarHoverText", $"{prefix}SidebarHoverText");
        }

        public void ApplyColorTheme(string themeName)
        {
            var theme = ThemePresets.GetTheme(themeName);
            if (theme == null) return;

            CurrentColorTheme = themeName;
            _currentThemeObject = theme;
            ApplyColorTheme(theme);

            SettingsService.Instance.SaveColorTheme(themeName, null);
        }

        public void ApplyCustomColorTheme(Color primary, Color secondary, Color accent)
        {
            var theme = new AppColorTheme
            {
                Primary = primary,
                Secondary = secondary,
                Accent = accent
            };

            CurrentColorTheme = "custom";
            _currentThemeObject = theme;
            ApplyColorTheme(theme);

            SettingsService.Instance.SaveColorTheme("custom", theme);
        }

        private void ApplyColorTheme(AppColorTheme theme)
        {
            string prefix = CurrentTheme == ThemeMode.Light ? "Light" : "Dark";

            // ========== LIGHT MODE ==========
            if (CurrentTheme == ThemeMode.Light)
            {
                // Background colors
                Color lightest = LightenColor(theme.Secondary, 0.9);
                Color lighter = LightenColor(theme.Secondary, 0.8);
                Color light = LightenColor(theme.Secondary, 0.7);

                // Gradients
                UpdateGradientBrush(_appResources, "LightMainBackground", lightest, lighter, light);
                UpdateGradientBrush(_appResources, "LightNavbarBackground", theme.Accent, theme.Primary);
                UpdateGradientBrush(_appResources, "LightToolbarBackground", lightest, light);
                UpdateGradientBrush(_appResources, "LightSidebarBackground", Color.FromRgb(255, 255, 255), lightest);
                UpdateGradientBrush(_appResources, "LightCardBackground", Color.FromRgb(255, 255, 255), lightest);
                UpdateGradientBrush(_appResources, "LightWordItemBackground", lightest, light);

                Color hoverColor1 = LightenColor(theme.Secondary, 0.6);
                Color hoverColor2 = LightenColor(theme.Secondary, 0.65);
                UpdateGradientBrush(_appResources, "LightWordItemHover", hoverColor1, hoverColor2);
                UpdateGradientBrush(_appResources, "LightSidebarHover", lightest, light);

                // Solid colors
                UpdateSolidBrush(_appResources, "LightTextColor", theme.Accent);
                UpdateSolidBrush(_appResources, "LightBorderColor", theme.Secondary);
                UpdateSolidBrush(_appResources, "LightButtonColor", theme.Primary);
                UpdateSolidBrush(_appResources, "LightWordBorder", theme.Primary);
                UpdateSolidBrush(_appResources, "LightMeaningBorderLeft", theme.Accent);
                UpdateSolidBrush(_appResources, "LightExampleBorder", theme.Secondary);
                UpdateSolidBrush(_appResources, "LightThemeSliderBackground", theme.Secondary);
                UpdateSolidBrush(_appResources, "LightThemeIconColor", theme.Primary);
                UpdateSolidBrush(_appResources, "LightSidebarHoverText", theme.Accent);

                // ✅ THÊM: Suggestions Popup Colors (Light Mode)
                UpdateSolidBrush(_appResources, "LightSuggestionsBackground", Color.FromRgb(255, 255, 255));
                UpdateSolidBrush(_appResources, "LightSuggestionsBorder", theme.Secondary);
                UpdateSolidBrush(_appResources, "LightSuggestionsItemBorder", lightest);
                UpdateSolidBrush(_appResources, "LightSuggestionsItemHover", lightest);
                UpdateSolidBrush(_appResources, "LightSuggestionsItemSelected", hoverColor1);

                // Search & Tool buttons
                UpdateGradientBrush(_appResources, "LightSearchButton", theme.Primary, theme.Secondary);
                UpdateGradientBrush(_appResources, "LightSearchButtonHover", theme.Accent, theme.Primary);
                UpdateGradientBrush(_appResources, "LightToolButtonActive", theme.Primary, theme.Secondary);
            }
            // ========== DARK MODE ==========
            else
            {
                Color darkest = Color.FromRgb(13, 27, 42);
                Color darker = Color.FromRgb(27, 38, 59);
                Color dark = Color.FromRgb(30, 41, 59);

                // Gradients
                UpdateGradientBrush(_appResources, "DarkMainBackground", darkest, darker);
                UpdateGradientBrush(_appResources, "DarkNavbarBackground",
                    DarkenColor(theme.Accent, 0.5), DarkenColor(theme.Primary, 0.5));
                UpdateGradientBrush(_appResources, "DarkToolbarBackground", dark, Color.FromRgb(51, 65, 85));
                UpdateGradientBrush(_appResources, "DarkSidebarBackground", dark, Color.FromRgb(15, 23, 42));
                UpdateGradientBrush(_appResources, "DarkCardBackground", dark, Color.FromRgb(51, 65, 85));

                UpdateSolidBrush(_appResources, "DarkWordItemBackground", Color.FromRgb(45, 55, 72));
                UpdateSolidBrush(_appResources, "DarkWordItemHover", Color.FromRgb(55, 65, 81));
                UpdateSolidBrush(_appResources, "DarkSidebarHover", Color.FromRgb(51, 65, 85));

                // Solid colors
                UpdateSolidBrush(_appResources, "DarkTextColor", theme.Secondary);
                UpdateSolidBrush(_appResources, "DarkBorderColor", theme.Primary);
                UpdateSolidBrush(_appResources, "DarkButtonColor", theme.Secondary);
                UpdateSolidBrush(_appResources, "DarkWordBorder", theme.Secondary);
                UpdateSolidBrush(_appResources, "DarkMeaningBorderLeft", theme.Primary);
                UpdateSolidBrush(_appResources, "DarkExampleBorder", theme.Secondary);
                UpdateSolidBrush(_appResources, "DarkThemeSliderBackground", theme.Primary);
                UpdateSolidBrush(_appResources, "DarkThemeIconColor", theme.Secondary);
                UpdateSolidBrush(_appResources, "DarkSidebarHoverText", theme.Secondary);

                // ✅ THÊM: Suggestions Popup Colors (Dark Mode)
                UpdateSolidBrush(_appResources, "DarkSuggestionsBackground", dark);
                UpdateSolidBrush(_appResources, "DarkSuggestionsBorder", theme.Primary);
                UpdateSolidBrush(_appResources, "DarkSuggestionsItemBorder", Color.FromRgb(51, 65, 85));
                UpdateSolidBrush(_appResources, "DarkSuggestionsItemHover", Color.FromRgb(51, 65, 85));
                UpdateSolidBrush(_appResources, "DarkSuggestionsItemSelected", Color.FromRgb(71, 85, 105));

                // Search & Tool buttons
                UpdateGradientBrush(_appResources, "DarkSearchButton", theme.Primary, theme.Secondary);
                UpdateGradientBrush(_appResources, "DarkSearchButtonHover",
                    DarkenColor(theme.Primary, 0.2), theme.Primary);
                UpdateGradientBrush(_appResources, "DarkToolButtonActive", theme.Primary, theme.Secondary);
            }

            ApplyTheme(CurrentTheme);
            System.Diagnostics.Debug.WriteLine($"✅ Applied color theme: {CurrentColorTheme} ({CurrentTheme} mode)");
        }

        // ========== HELPER METHODS ==========

        private Color LightenColor(Color color, double factor)
        {
            return Color.FromRgb(
                (byte)(color.R + (255 - color.R) * factor),
                (byte)(color.G + (255 - color.G) * factor),
                (byte)(color.B + (255 - color.B) * factor)
            );
        }

        private Color DarkenColor(Color color, double factor)
        {
            return Color.FromRgb(
                (byte)(color.R * (1 - factor)),
                (byte)(color.G * (1 - factor)),
                (byte)(color.B * (1 - factor))
            );
        }

        private void UpdateResource(string key, string sourceKey)
        {
            if (_appResources.Contains(sourceKey))
            {
                _appResources[key] = _appResources[sourceKey];
            }
        }

        private void UpdateSolidBrush(ResourceDictionary resources, string key, Color color)
        {
            resources[key] = new SolidColorBrush(color);
        }

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

        private void ReloadDefaultColors(ThemeMode mode)
        {
            try
            {
                var colorsDict = new ResourceDictionary
                {
                    Source = new Uri("Resources/Styles/Colors.xaml", UriKind.Relative)
                };

                string prefix = mode == ThemeMode.Light ? "Light" : "Dark";

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
                    "SearchButtonHover", 
                    // ✅ THÊM: Suggestions resources
                    "SuggestionsBackground", "SuggestionsBorder",
                    "SuggestionsItemBorder", "SuggestionsItemHover", "SuggestionsItemSelected",
                    "ToolButtonActive", "NavButtonColor",
                    "NavButtonHover", "HamburgerBackground", "HamburgerHover",
                    "HamburgerIcon", "ThemeToggleBackground", "ThemeSliderBackground",
                    "ThemeIconColor", "ToolbarBorder", "SidebarHover", "SidebarHoverText"
                };

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

        public void ResetToDefaultColors()
        {
            try
            {
                var colorsDict = new ResourceDictionary
                {
                    Source = new Uri("Resources/Styles/Colors.xaml", UriKind.Relative)
                };

                string prefix = CurrentTheme == ThemeMode.Dark ? "Dark" : "Light";

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
                    "SearchButtonHover",
                    // ✅ THÊM: Suggestions resources
                    "SuggestionsBackground", "SuggestionsBorder",
                    "SuggestionsItemBorder", "SuggestionsItemHover", "SuggestionsItemSelected",
                    "ToolButtonActive", "NavButtonColor",
                    "NavButtonHover", "HamburgerBackground", "HamburgerHover",
                    "HamburgerIcon", "ThemeToggleBackground", "ThemeSliderBackground",
                    "ThemeIconColor", "ToolbarBorder", "SidebarHover", "SidebarHoverText"
                };

                foreach (var key in resourcesToReset)
                {
                    string sourceKey = $"{prefix}{key}";
                    if (colorsDict.Contains(sourceKey))
                    {
                        _appResources[key] = colorsDict[sourceKey];
                        System.Diagnostics.Debug.WriteLine($"  ✅ Reset {key} from {sourceKey}");
                    }
                }

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

        private void UpdateSearchInputColor()
        {
            try
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow == null) return;

                var searchInput = mainWindow.FindName("SearchInput") as System.Windows.Controls.TextBox;
                if (searchInput == null) return;

                var searchTextBrush = _appResources["SearchText"] as Brush;
                var placeholderBrush = _appResources["SearchPlaceholder"] as Brush;

                if (searchInput.Text == "Nhập từ cần tra...")
                {
                    searchInput.Foreground = placeholderBrush;
                }
                else if (!string.IsNullOrEmpty(searchInput.Text))
                {
                    searchInput.Foreground = searchTextBrush;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Updated SearchInput color (Mode: {CurrentTheme})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Failed to update SearchInput color: {ex.Message}");
            }
        }
    }
}