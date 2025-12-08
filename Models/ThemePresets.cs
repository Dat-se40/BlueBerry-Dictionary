using System.Collections.Generic;
using System.Windows.Media;

namespace BlueBerryDictionary.Models
{
    public class AppColorTheme
    {
        public Color Primary { get; set; }
        public Color Secondary { get; set; }
        public Color Accent { get; set; }
    }

    /// <summary>
    /// 18 themes có sẵn
    /// </summary>
    public static class ThemePresets
    {
        private static readonly Dictionary<string, AppColorTheme> Themes = new()
        {
            // ===== THEME 1: PASTEL DREAM =====
            ["theme1"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#A2D2FF"),
                Secondary = (Color)ColorConverter.ConvertFromString("#BDE0FE"),
                Accent = (Color)ColorConverter.ConvertFromString("#CDB4DB")
            },

            // ===== THEME 2: LAVENDER MIST =====
            ["theme2"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#C8B6FF"),
                Secondary = (Color)ColorConverter.ConvertFromString("#E7C6FF"),
                Accent = (Color)ColorConverter.ConvertFromString("#B8C0FF")
            },

            // ===== THEME 3: AQUA FRESH =====
            ["theme3"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#3DCCC7"),
                Secondary = (Color)ColorConverter.ConvertFromString("#68D8D6"),
                Accent = (Color)ColorConverter.ConvertFromString("#07BEB8")
            },

            // ===== THEME 4: ANIME DARK =====
            ["theme4"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#FF6666"),
                Secondary = (Color)ColorConverter.ConvertFromString("#FF9999"),
                Accent = (Color)ColorConverter.ConvertFromString("#FF3333")
            },

            // ===== THEME 5: PURPLE HAZE =====
            ["theme5"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#FFD6FF"),
                Secondary = (Color)ColorConverter.ConvertFromString("#C8B6FF"),
                Accent = (Color)ColorConverter.ConvertFromString("#B8C0FF")
            },

            // ===== THEME 6: ALICE BLUE =====
            ["theme6"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#ABC4FF"),
                Secondary = (Color)ColorConverter.ConvertFromString("#CCDBFD"),
                Accent = (Color)ColorConverter.ConvertFromString("#B6CCFE")
            },

            // ===== THEME 7: CHERRY BLOSSOM =====
            ["theme7"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#C9184A"),
                Secondary = (Color)ColorConverter.ConvertFromString("#FF758F"),
                Accent = (Color)ColorConverter.ConvertFromString("#590D22")
            },

            // ===== THEME 8: FOREST SAGE =====
            ["theme8"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#52796F"),
                Secondary = (Color)ColorConverter.ConvertFromString("#84A98C"),
                Accent = (Color)ColorConverter.ConvertFromString("#354F52")
            },

            // ===== THEME 9: OCEAN GRADIENT =====
            ["theme9"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#34A0A4"),
                Secondary = (Color)ColorConverter.ConvertFromString("#52B69A"),
                Accent = (Color)ColorConverter.ConvertFromString("#1E6091")
            },

            // ===== THEME 10: VIOLET SUNSET =====
            ["theme10"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#5E60CE"),
                Secondary = (Color)ColorConverter.ConvertFromString("#5390D9"),
                Accent = (Color)ColorConverter.ConvertFromString("#7400B8")
            },

            // ===== THEME 11: SAGE GREEN =====
            ["theme11"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#A1BC98"),
                Secondary = (Color)ColorConverter.ConvertFromString("#D2DCB6"),
                Accent = (Color)ColorConverter.ConvertFromString("#778873")
            },

            // ===== THEME 12: NEON CYBERPUNK =====
            ["theme12"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#63C8FF"),
                Secondary = (Color)ColorConverter.ConvertFromString("#4DFFBE"),
                Accent = (Color)ColorConverter.ConvertFromString("#FF2DD1")
            },

            // ===== THEME 13: PASTEL SKY =====
            ["theme13"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#81BFDA"),
                Secondary = (Color)ColorConverter.ConvertFromString("#B1F0F7"),
                Accent = (Color)ColorConverter.ConvertFromString("#FADA7A")
            },

            // ===== THEME 14: CANDY DREAM =====
            ["theme14"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#DD7BDF"),
                Secondary = (Color)ColorConverter.ConvertFromString("#FFBBE1"),
                Accent = (Color)ColorConverter.ConvertFromString("#B3BFFF")
            },

            // ===== THEME 15: PEACH BLOSSOM =====
            ["theme15"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#FFA4A4"),
                Secondary = (Color)ColorConverter.ConvertFromString("#FFBDBD"),
                Accent = (Color)ColorConverter.ConvertFromString("#BADFDB")
            },

            // ===== THEME 16: SOFT RAINBOW =====
            ["theme16"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#C2E2FA"),
                Secondary = (Color)ColorConverter.ConvertFromString("#FFF1CB"),
                Accent = (Color)ColorConverter.ConvertFromString("#FF8F8F")
            },

            // ===== THEME 17: LATTE =====
            ["theme17"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#DCC5B2"),
                Secondary = (Color)ColorConverter.ConvertFromString("#F0E4D3"),
                Accent = (Color)ColorConverter.ConvertFromString("#D9A299")
            },

            // ===== THEME 18: FOREST MEADOW =====
            ["theme18"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#6F826A"),
                Secondary = (Color)ColorConverter.ConvertFromString("#BBD8A3"),
                Accent = (Color)ColorConverter.ConvertFromString("#BF9264")
            },


            // ===== THEME 19: TROPICAL SUNSET =====
            ["theme19"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#FFCF96"),
                Secondary = (Color)ColorConverter.ConvertFromString("#F6FDC3"),
                Accent = (Color)ColorConverter.ConvertFromString("#FF8080")
            },

            // ===== THEME 20: SOFT BLOSSOM =====
            ["theme20"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#F8EDED"),
                Secondary = (Color)ColorConverter.ConvertFromString("#F6DFEB"),
                Accent = (Color)ColorConverter.ConvertFromString("#E4BAD4")
            },

            // ===== THEME 21: PEACHY CREAM =====
            ["theme21"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#F4BFBF"),
                Secondary = (Color)ColorConverter.ConvertFromString("#FFD9C0"),
                Accent = (Color)ColorConverter.ConvertFromString("#8CC0DE")
            },

            // ===== THEME 22: TEAL ROSE =====
            ["theme22"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#568F87"),
                Secondary = (Color)ColorConverter.ConvertFromString("#F5BABB"),
                Accent = (Color)ColorConverter.ConvertFromString("#064232")
            },

            // ===== THEME 23: FRESH MINT =====
            ["theme23"] = new AppColorTheme
            {
                Primary = (Color)ColorConverter.ConvertFromString("#98EECC"),
                Secondary = (Color)ColorConverter.ConvertFromString("#D0F5BE"),
                Accent = (Color)ColorConverter.ConvertFromString("#79E0EE")
            }
        };

        public static AppColorTheme GetTheme(string name)
        {
            return Themes.TryGetValue(name.ToLower(), out var theme) ? theme : Themes["theme1"];
        }

        public static IEnumerable<string> GetAllThemeNames()
        {
            return Themes.Keys;
        }
    }
}
