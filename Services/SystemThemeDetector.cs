using Microsoft.Win32;
using System;

namespace BlueBerryDictionary.Services
{
    /// <summary>
    /// Detect và theo dõi theme của Windows 10/11
    /// Thay đổi mode theo thời gian thực
    /// </summary>
    public static class SystemThemeDetector
    {
        // Event khi system theme thay đổi
        public static event EventHandler<bool> SystemThemeChanged;

        private static bool _isWatching = false;

        /// <summary>
        /// Kiểm tra xem Windows đang dùng Dark Mode không
        /// </summary>
        public static bool IsSystemDarkMode()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

                var value = key?.GetValue("AppsUseLightTheme");

                return value is int intValue && intValue == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [SystemThemeDetector] Error: {ex.Message}");
                return false;
            }
        }

        // Bắt đầu theo dõi thay đổi theme
        public static void StartWatching()
        {
            if (_isWatching)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ [SystemThemeDetector] Already watching");
                return;
            }

            try
            {
                SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
                _isWatching = true;
                System.Diagnostics.Debug.WriteLine("✅ [SystemThemeDetector] Started watching");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [SystemThemeDetector] Start error: {ex.Message}");
            }
        }

        // Dừng theo dõi
        public static void StopWatching()
        {
            if (!_isWatching) return;

            try
            {
                SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
                _isWatching = false;
                System.Diagnostics.Debug.WriteLine("✅ [SystemThemeDetector] Stopped watching");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [SystemThemeDetector] Stop error: {ex.Message}");
            }
        }

        // Handler khi theme thay đổi
        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                bool isDark = IsSystemDarkMode();
                System.Diagnostics.Debug.WriteLine($"🔄 [SystemThemeDetector] Theme changed → Dark: {isDark}");
                SystemThemeChanged?.Invoke(null, isDark);
            }
        }
    }
}
