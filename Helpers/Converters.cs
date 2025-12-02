using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlueBerryDictionary.Helpers
{
    /// <summary>
    /// Converter: bool → Visibility (ngược lại)
    /// true → Collapsed, false → Visible
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Collapsed;
        }
    }
}
