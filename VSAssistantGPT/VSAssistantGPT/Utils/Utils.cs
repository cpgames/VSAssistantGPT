using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using cpGames.VSA.ViewModel;
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
            var dirInfo = dirSubs.Length == 0
                ? Directory.CreateDirectory(Path.Combine(dir, subFolder))
                : new DirectoryInfo(dirSubs[0]);
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
        #region IValueConverter Members
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var imageName = (string)value!;
            return $"pack://application:,,,/VSA;component/Resources/icons/{imageName}.png";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (bool)value! ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (Visibility)value! == Visibility.Visible;
        }
        #endregion
    }

    public class BoolToVisibilityInvConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (bool)value! ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (Visibility)value! == Visibility.Collapsed;
        }
        #endregion
    }

    public class BoolToInvBoolConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !(bool)value!;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !(bool)value!;
        }
        #endregion
    }

    public class PathToFolderConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var path = (string)value!;
            var parts = path.Split('\\');
            return parts.Last();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class FileStatusToTextConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var status = (FileViewModel.FileStatus)value!;
            return status switch
            {
                FileViewModel.FileStatus.NotSynced => "Not Synced",
                FileViewModel.FileStatus.Synced => "Synced",
                FileViewModel.FileStatus.Syncing => "Syncing",
                FileViewModel.FileStatus.Deleting => "Deleting",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class FileStatusToColorConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var status = (FileViewModel.FileStatus)value!;
            return status switch
            {
                FileViewModel.FileStatus.NotSynced => "Red",
                FileViewModel.FileStatus.Synced => "Green",
                FileViewModel.FileStatus.Syncing => "Yellow",
                FileViewModel.FileStatus.Deleting => "Yellow",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class SyncedFileStatusToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var status = (FileViewModel.FileStatus)value!;
            return status == FileViewModel.FileStatus.Synced ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (Visibility)value! == Visibility.Visible
                ? FileViewModel.FileStatus.Synced
                : FileViewModel.FileStatus.NotSynced;
        }
        #endregion
    }

    public class NotSyncedFileStatusToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var status = (FileViewModel.FileStatus)value!;
            return status == FileViewModel.FileStatus.NotSynced ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (Visibility)value! == Visibility.Visible
                ? FileViewModel.FileStatus.NotSynced
                : FileViewModel.FileStatus.Synced;
        }
        #endregion
    }
}