namespace BlueBerryDictionary.Models
{
    /// <summary>
    /// Model lưu tất cả settings của app
    /// </summary>
    public class AppSettings
    {
        // Theme
        public string ThemeMode { get; set; } = "Light"; // "Light" | "Dark" | "Auto"
        public string ColorTheme { get; set; } = "blue";
        public AppColorTheme CustomColorTheme { get; set; }


        // Appearance
        public string FontFamily { get; set; } = "Segoe UI";
        public double FontSize { get; set; } = 14;

        // Data
        public bool AutoSaveHistory { get; set; } = true;
        public int FavouriteLimit { get; set; } = 500;
    }
}
