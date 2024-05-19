using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public static string GetRelativePath(string filePath, string folderPath)
        {
            var fileUri = new Uri(filePath);
            var folderUri = new Uri(folderPath);
            var relativeUri = folderUri.MakeRelativeUri(fileUri);
            return Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
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
}