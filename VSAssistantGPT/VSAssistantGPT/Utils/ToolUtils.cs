using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace cpGames.VSA
{
    public static class ToolUtils
    {
        #region Methods
        public static SolidColorBrush GetColor(string category, bool dark)
        {
            var hash = category.GetHashCode();
            var r = (byte)(hash & 0xFF);
            var g = (byte)((hash >> 8) & 0xFF);
            var b = (byte)((hash >> 16) & 0xFF);
            if (dark)
            {
                r = (byte)(r * 0.4);
                g = (byte)(g * 0.4);
                b = (byte)(b * 0.4);
            }
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        #endregion
    }

    public class ToolCategoryToColorConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            if (value is string category)
            {
                return ToolUtils.GetColor(category, true);
            }
            return new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        public object? ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
}