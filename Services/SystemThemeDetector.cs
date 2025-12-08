using Microsoft.Win32;

namespace BlueBerryDictionary.Services
{
    /// <summary>
    /// Detect theme của Windows 10/11
    /// </summary>
    public static class SystemThemeDetector
    {
        public static bool IsSystemDarkMode()
        {
            try
            {
                // Đọc registry key của Windows
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

                var value = key?.GetValue("AppsUseLightTheme");

                // 0 = Dark mode, 1 = Light mode
                return value is int intValue && intValue == 0;
            }
            catch
            {
                return false; // Mặc định Light mode nếu lỗi
            }
        }
    }
}
