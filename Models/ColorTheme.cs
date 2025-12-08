using System.Windows.Media;

namespace BlueBerryDictionary.Models
{
    /// <summary>
    /// Đại diện cho 1 bộ màu (gồm 3 màu)
    /// Dùng cho cả Preset và Custom theme
    /// </summary>
    public class ColorTheme
    {
        // Màu chính (dùng cho button, border chính)
        public Color Primary { get; set; }

        // Màu phụ (dùng cho gradient, hover)
        public Color Secondary { get; set; }

        // Màu nhấn (dùng cho navbar, dark elements)
        public Color Accent { get; set; }
    }
}
