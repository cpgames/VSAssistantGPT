using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using PCRE;

namespace cpGames.VSA
{
    public static class Utils
    {
        #region Methods
        public static string GetOrCreateAppDir(string? subFolder = null)
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            dir = Path.Combine(dir, "cpGames", "VSA");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (subFolder == null)
            {
                return dir;
            }
            var dirSubs = Directory.GetDirectories(dir, subFolder);
            var dirInfo = dirSubs.Length == 0 ?
                Directory.CreateDirectory(Path.Combine(dir, subFolder)) :
                new DirectoryInfo(dirSubs[0]);
            return dirInfo.FullName;
        }

        public static IEnumerable<string> GetFiles(string folderPath, string extension)
        {
            return Directory.GetFiles(folderPath, $"*{extension}");
        }

        public static void OpenInExplorer(string path)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
        }

        public static bool StringMatchesRegex(string str, string pattern)
        {
            return
                string.IsNullOrEmpty(pattern) ||
                PcreRegex.IsMatch(str, pattern);
        }
        #endregion
    }

    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string imageName = (string)value!;
            return $"pack://application:,,,/VSA;component/Resources/icons/{imageName}.png";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (bool)value! ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (System.Windows.Visibility)value! == System.Windows.Visibility.Visible;
        }
    }

    public class BoolToVisibilityInvConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (bool)value! ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (System.Windows.Visibility)value! == System.Windows.Visibility.Collapsed;
        }
    }

    public class BoolToInvBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !(bool)value!;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !(bool)value!;
        }
    }
}