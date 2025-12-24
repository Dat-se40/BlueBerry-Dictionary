using BlueBerryDictionary.Services;
using BlueBerryDictionary.Services.Network;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BlueBerryDictionary
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. LOAD SETTINGS TRƯỚC (set resources globally)
            LoadSettings();

            // 2. SET SHUTDOWN MODE
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // 3. CHECK LOGIN & SHOW WINDOWS
            bool isLoggedIn = CheckLoginState();

            if (!isLoggedIn)
            {
                var loginWindow = new LoginWindow();
                bool? loginResult = loginWindow.ShowDialog();

                if (loginResult == true)
                {
                    AsyncData();
                    System.Diagnostics.Debug.WriteLine("✅ User logged in → Showing MainWindow");
                }
                else if (loginResult == false)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Guest mode → Showing MainWindow");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ User cancelled login → Exit app");
                    this.Shutdown();
                    return;
                }

                ShowMainWindow();
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("✅ Already logged in → Showing MainWindow");
                ShowMainWindow();
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
        }

        /// <summary>
        /// Check if user is logged in
        /// </summary>

        private bool CheckLoginState()
        {
            return GoogleAuthService.Instance.TrySilentLoginAsync().Result;
        }

        /// <summary>
        /// Async data sync
        /// </summary>
        private async void AsyncData()
        {
            var instance = CloudSyncService.Instance;

            try
            {
                string mywordsPath = instance.GetLocalFilePath(CloudSyncService.essentialFile[0]);
                string tagsPath = instance.GetLocalFilePath(CloudSyncService.essentialFile[1]);

                string mywordsJson = await File.ReadAllTextAsync(mywordsPath);
                string tagsJson = await File.ReadAllTextAsync(tagsPath);

                await Task.WhenAll(
                    instance.MergeMyWordsAsync(mywordsJson),
                    instance.MergeTagsAsync(tagsJson)
                );

                Console.WriteLine("✅ Async data merge completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Async data failed: {ex.Message}");
                MessageBox.Show($"Lỗi đồng bộ dữ liệu:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Show MainWindow
        /// </summary>
        private void ShowMainWindow()
        {
            var mainWindow = new MainWindow();

            // Apply font to MainWindow
            try
            {
                mainWindow.FontFamily = (FontFamily)Current.Resources["AppFontFamily"];
                mainWindow.FontSize = (double)Current.Resources["AppFontSize"];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to apply font to MainWindow: {ex.Message}");
            }

            this.MainWindow = mainWindow;
            mainWindow.Show();
        }

        /// <summary>
        /// Load settings và apply khi app khởi động
        /// </summary>
        private void LoadSettings()
        {
            var settings = Services.SettingsService.Instance.CurrentSettings;

            // ===== APPLY FONT GLOBALLY =====
            try
            {
                string fontFamily = string.IsNullOrEmpty(settings.FontFamily) ? "Segoe UI" : settings.FontFamily;
                double fontSize = settings.FontSize > 0 ? settings.FontSize : 14;

                Current.Resources["AppFontFamily"] = new FontFamily(fontFamily);
                Current.Resources["AppFontSize"] = fontSize;

                System.Diagnostics.Debug.WriteLine($"✅ Loaded font: {fontFamily} ({fontSize}pt)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to load font: {ex.Message}");
                // Fallback to default
                Current.Resources["AppFontFamily"] = new FontFamily("Segoe UI");
                Current.Resources["AppFontSize"] = 14.0;
            }

            // ===== APPLY THEME MODE =====
            var themeMode = settings.ThemeMode switch
            {
                "Light" => Services.ThemeMode.Light,
                "Dark" => Services.ThemeMode.Dark,
                "Auto" => Services.ThemeMode.Auto,
                _ => Services.ThemeMode.Light
            };
            Services.ThemeManager.Instance.SetThemeMode(themeMode);

            // ===== APPLY COLOR THEME =====
            if (settings.ColorTheme == "custom" && settings.CustomColorTheme != null)
            {
                var theme = settings.CustomColorTheme;
                Services.ThemeManager.Instance.ApplyCustomColorTheme(
                    theme.Primary,
                    theme.Secondary,
                    theme.Accent
                );
                System.Diagnostics.Debug.WriteLine("✅ Loaded custom color theme");
            }
            else if (settings.ColorTheme == "default" || string.IsNullOrEmpty(settings.ColorTheme))
            {
                // Reset về màu mặc định trong Colors.xaml
                Services.ThemeManager.Instance.ResetToDefaultColors();
                System.Diagnostics.Debug.WriteLine("✅ Loaded default colors from Colors.xaml");
            }
            else
            {
                // Load preset theme (theme1, theme2, ...)
                Services.ThemeManager.Instance.ApplyColorTheme(settings.ColorTheme);
                System.Diagnostics.Debug.WriteLine($"✅ Loaded preset theme: {settings.ColorTheme}");
            }

        }


        /// <summary>
        /// Set empty background (for future use)
        /// </summary>
        private void SetEmptyBackground()
        {
            var emptyBrush = new ImageBrush
            {
                Stretch = Stretch.UniformToFill,
                Opacity = 0.15,
                ImageSource = null
            };
            Current.Resources["AppBackgroundImage"] = emptyBrush;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Cleanup: Stop watching system theme
            SystemThemeDetector.StopWatching();

            base.OnExit(e);
        }
    }
}
