using BlueBerryDictionary.Models;
using System;
using System.IO;
using System.Text.Json;

namespace BlueBerryDictionary.Services
{
    /// <summary>
    /// Lưu/Load settings từ file JSON
    /// Singleton pattern
    /// </summary>
    public class SettingsService
    {
        private static SettingsService _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private readonly string _settingsPath;
        public AppSettings CurrentSettings { get; private set; }

        private SettingsService()
        {
            // Lưu vào %LocalAppData%/BlueBerryDictionary/AppSettings.json
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BlueBerryDictionary",
                "AppSettings.json"
            );

            LoadSettings();
        }

        /// <summary>
        /// Load settings từ file (hoặc tạo mới nếu chưa có)
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    CurrentSettings = JsonSerializer.Deserialize<AppSettings>(json);
                }
                else
                {
                    CurrentSettings = new AppSettings(); // Default settings
                }
            }
            catch
            {
                CurrentSettings = new AppSettings();
            }
        }

        /// <summary>
        /// Lưu settings vào file JSON
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));
                string json = JsonSerializer.Serialize(CurrentSettings, new JsonSerializerOptions
                {
                    WriteIndented = true // JSON dễ đọc
                });
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Lỗi lưu settings: {ex.Message}");
            }
        }

        #region Shortcut methods
        public void SaveThemeMode(Services.ThemeMode mode)
        {
            CurrentSettings.ThemeMode = mode.ToString();
            SaveSettings();
        }

        public void SaveColorTheme(string themeName, AppColorTheme customTheme)
        {
            CurrentSettings.ColorTheme = themeName;
            CurrentSettings.CustomColorTheme = customTheme;
            SaveSettings();
        }

        public void SaveFontFamily(string fontFamily)
        {
            CurrentSettings.FontFamily = fontFamily;
            SaveSettings();
        }

        public void SaveFontSize(double fontSize)
        {
            CurrentSettings.FontSize = fontSize;
            SaveSettings();
        }
        #endregion
    }
}
