using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA
{
    public static class DTEUtils
    {
        #region Fields
        public const string SUPPORTED_FILE_EXTENSIONS_REGEX = @"\.(c|cs|cpp|doc|docx|h|html|java|json|md|pdf|php|pptx|py|rb|tex|txt|css|js|sh|ts)\b";
        #endregion

        #region Methods
        public static TextDocument GetActiveDocument()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            if (dte.ActiveDocument?.Object() is not TextDocument textDocument)
            {
                throw new Exception("No active document found.");
            }
            return textDocument;
        }

        public static string GetActiveDocumentText()
        {
            var document = GetActiveDocument();
            return document.StartPoint.CreateEditPoint().GetText(document.EndPoint);
        }

        public static void SetActiveDocumentText(string text)
        {
            var document = GetActiveDocument();
            document.StartPoint.CreateEditPoint().ReplaceText(document.EndPoint, text, 0);
            document.Parent.Save();
        }

        public static string GetActiveDocumentRelativePath()
        {
            var document = GetActiveDocument();
            return document.Parent.FullName.Replace(GetProjectPath(), "");
        }

        public static bool SetSelection(string data)
        {
            var document = GetActiveDocument();
            var originalText = document.Selection.Text;
            if (originalText != data)
            {
                document.Selection.Insert(data);
                return true;
            }
            return false;
        }

        public static string GetSelection()
        {
            var document = GetActiveDocument();
            return document.Selection.Text;
        }

        public static bool IsSelectionEmpty()
        {
            var document = GetActiveDocument();
            return document.Selection.IsEmpty;
        }

        public static Project? GetActiveProject()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            if (dte.ActiveSolutionProjects is not Array projects || projects.Length == 0)
            {
                return null;
            }
            return projects.GetValue(0) as Project;
        }

        public static List<ProjectItem> GetProjectItemsInActiveProject()
        {
            var project = GetActiveProject();
            if (project == null)
            {
                throw new Exception("No active project found.");
            }
            var projectItems = new List<ProjectItem>();
            var items = project.ProjectItems.GetEnumerator();
            while (items.MoveNext())
            {
                var currentItem = items.Current as ProjectItem;
                var childItem = GetProjectItemsInProjectItem(currentItem, projectItems);
                AppendProjectItem(childItem, projectItems);
            }
            return projectItems;
        }

        public static string GetRelativePath(string path)
        {
            var relativePath = path.Replace(GetProjectPath(), "");
            ValidateFileName(ref relativePath);
            return relativePath;
        }

        public static List<string> GetFilesInActiveProject()
        {
            var projectItems = GetProjectItemsInActiveProject();
            var files = new List<string>();
            foreach (var item in projectItems)
            {
                var fileName = GetRelativePath(item.FileNames[0]);
                files.Add(fileName);
            }
            return files;
        }

        public static ProjectItem? GetProjectItemsInProjectItem(ProjectItem? item, List<ProjectItem> projectItems)
        {
            if (item?.ProjectItems == null)
            {
                return item;
            }

            var items = item.ProjectItems.GetEnumerator();
            while (items.MoveNext())
            {
                var currentItem = (ProjectItem)items.Current!;
                var childItem = GetProjectItemsInProjectItem(currentItem, projectItems);
                AppendProjectItem(childItem, projectItems);
            }
            return item;
        }

        private static void AppendProjectItem(ProjectItem? item, ICollection<ProjectItem> projectItems)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (item is { Kind: Constants.vsProjectItemKindPhysicalFile } &&
                Utils.StringMatchesRegex(item.Name, SUPPORTED_FILE_EXTENSIONS_REGEX))
            {
                projectItems.Add(item);
            }
        }

        public static string GetProjectPath()
        {
            var project = GetActiveProject();
            if (project == null)
            {
                throw new Exception("No active project found.");
            }
            return Path.GetDirectoryName(project.FullName)!;
        }

        public static void ValidateFileName(ref string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new Exception("File name cannot be empty.");
            }
            if (fileName.StartsWith("/"))
            {
                fileName = fileName.Substring(1);
            }
            else if (fileName.StartsWith("\\"))
            {
                fileName = fileName.Substring(1);
            }
        }

        public static void CreateFolder(string fileName)
        {
            ValidateFileName(ref fileName);
            var project = GetActiveProject();
            if (project == null)
            {
                throw new Exception("No active project found.");
            }
            var fullPath = Path.Combine(GetProjectPath(), fileName);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            project.ProjectItems.AddFromDirectory(fullPath);
        }

        public static void CreateFile(string fileName)
        {
            ValidateFileName(ref fileName);
            var project = GetActiveProject();
            if (project == null)
            {
                throw new Exception("No active project found.");
            }
            var fullPath = Path.Combine(GetProjectPath(), fileName);
            if (!File.Exists(fullPath))
            {
                // create folder if doesn't exist
                var folder = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder!);
                }
                File.Create(fullPath).Close();
                project.ProjectItems.AddFromFile(fullPath);
            }
            OpenFile(fileName);
        }

        public static void DeleteFile(string fileName)
        {
            ValidateFileName(ref fileName);
            var project = GetActiveProject();
            if (project == null)
            {
                throw new Exception("No active project found.");
            }
            var fullPath = Path.Combine(GetProjectPath(), fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public static bool FileExists(string fileName)
        {
            ValidateFileName(ref fileName);
            var project = GetActiveProject();
            if (project == null)
            {
                throw new Exception("No active project found.");
            }
            var fullPath = Path.Combine(GetProjectPath(), fileName);
            return File.Exists(fullPath);
        }

        public static void OpenFile(string fileName)
        {
            ValidateFileName(ref fileName);
            if (!FileExists(fileName))
            {
                throw new Exception("File does not exist.");
            }
            var project = GetActiveProject();
            if (project == null)
            {
                throw new Exception("No active project found.");
            }
            var fullPath = Path.Combine(GetProjectPath(), fileName);
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            Assumes.Present(dte);

            dte.ItemOperations.OpenFile(fullPath);
        }

        public static void CloseFile(string fileName)
        {
            ValidateFileName(ref fileName);
            var project = GetActiveProject();
            if (project == null)
            {
                throw new Exception("No active project found.");
            }
            var fullPath = Path.Combine(GetProjectPath(), fileName);
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            var document = dte.Documents.Item(fullPath);
            document.Close();
        }

        public static void SaveFile(string? fileName = null)
        {
            if (fileName == null)
            {
                fileName = GetActiveDocumentRelativePath();
            }
            ValidateFileName(ref fileName);
            var fullPath = Path.Combine(GetProjectPath(), fileName);
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            var document = dte.Documents.Item(fullPath);
            document.Save();
        }

        public static string GetSolutionFolderPath()
        {
            var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            if (dte.Solution is not { IsOpen: true })
            {
                throw new InvalidOperationException("Solution is not open.");
            }
            var dir = Path.GetDirectoryName(dte.Solution.FullName);
            if (dir == null)
            {
                throw new InvalidOperationException("Solution directory is null.");
            }
            return dir;
        }

        public static bool IsSolutionOpen()
        {
            var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            return dte.Solution.IsOpen;
        }

        public static JArray GetErrors()
        {
            var errorArray = new JArray();
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            Assumes.Present(dte);
            var errorList = dte.ToolWindows.ErrorList;

            for (var i = 1; i <= errorList.ErrorItems.Count; i++)
            {
                var errorItem = errorList.ErrorItems.Item(i);
                var errorObject = new JObject
                {
                    ["Description"] = errorItem.Description,
                    ["ErrorLevel"] = errorItem.ErrorLevel.ToString(),
                    ["File"] = GetRelativePath(errorItem.FileName),
                    ["Line"] = errorItem.Line,
                    ["Column"] = errorItem.Column
                };

                errorArray.Add(errorObject);
            }

            return errorArray;
        }
        #endregion
    }
}