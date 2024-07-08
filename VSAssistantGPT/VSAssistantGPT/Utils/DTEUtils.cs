using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using Constants = EnvDTE.Constants;

namespace cpGames.VSA
{
    public static class DTEUtils
    {
        #region Fields
        public const string SUPPORTED_FILE_EXTENSIONS_REGEX =
            @"\.(c|cs|cpp|doc|docx|h|html|java|json|md|pdf|php|pptx|py|rb|tex|txt|css|js|sh|ts)\b";
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

        public static bool HasActiveDocument()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            return dte.ActiveDocument != null;
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

        public static string GetActiveDocumentPath()
        {
            var document = GetActiveDocument();
            return document.Parent.FullName;
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
            var document = dte.ActiveDocument;
            if (document != null)
            {
                return document.ProjectItem?.ContainingProject;
            }

            if (dte.ActiveSolutionProjects is not Array projects || projects.Length == 0)
            {
                return null;
            }

            return projects.GetValue(0) as Project;
        }

        public static List<Project> GetProjectsInSolution()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            var projects = new List<Project>();
            var solutionProjects = dte.Solution.Projects.GetEnumerator();
            try
            {
                while (solutionProjects.MoveNext())
                {
                    if (solutionProjects.Current is Project project)
                    {
                        RetrieveProjects(project, projects);
                    }
                }
            }
            finally
            {
                (solutionProjects as IDisposable)?.Dispose();
            }

            return projects;
        }

        private static void RetrieveProjects(Project? currentProject, List<Project> projects)
        {
            if (currentProject == null)
            {
                return;
            }

            projects.Add(currentProject);

            // Check for nested projects (virtual folders)
            if (currentProject.ProjectItems != null)
            {
                foreach (ProjectItem item in currentProject.ProjectItems)
                {
                    if (item.SubProject != null)
                    {
                        RetrieveProjects(item.SubProject, projects);
                    }
                }
            }
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

        public static List<ProjectItem> GetSolutionItems()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            Assumes.Present(dte);
            var solutionItems = new List<ProjectItem>();
            var uniqueItems = new HashSet<string>();

            var projects = dte.Solution.Projects.GetEnumerator();
            while (projects.MoveNext())
            {
                var project = projects.Current as Project;
                if (project == null)
                {
                    continue;
                }

                AddProjectItems(project, project.ProjectItems, solutionItems, uniqueItems);
            }

            return solutionItems;
        }

        private static void AddProjectItems(Project? currentProject, ProjectItems? projectItems,
            ICollection<ProjectItem> solutionItems, HashSet<string> uniqueItems)
        {
            if (projectItems == null || currentProject == null)
            {
                return;
            }

            foreach (ProjectItem projectItem in projectItems)
            {
                if (projectItem == null)
                {
                    continue;
                }

                // Check if the ProjectItem is part of the current project
                if (projectItem.ContainingProject != currentProject)
                {
                    continue;
                }

                if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFolder ||
                    projectItem.Kind == Constants.vsProjectItemKindVirtualFolder ||
                    projectItem.Kind == Constants.vsProjectItemKindSolutionItems)
                {
                    var subProjectItems = projectItem.ProjectItems;
                    var containingProject = currentProject;
                    if (subProjectItems == null)
                    {
                        subProjectItems = projectItem.SubProject?.ProjectItems;
                        containingProject = projectItem.SubProject;
                    }

                    AddProjectItems(containingProject, subProjectItems, solutionItems, uniqueItems);
                }
                else if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFile &&
                         Utils.StringMatchesRegex(projectItem.Name, SUPPORTED_FILE_EXTENSIONS_REGEX) &&
                         uniqueItems.Add(projectItem.FileNames[0]))
                {
                    solutionItems.Add(projectItem);
                }
            }
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

        public static string GetActiveProjectPath()
        {
            var project = GetActiveProject();
            if (project == null)
            {
                throw new Exception("No active project found.");
            }

            return Path.GetDirectoryName(project.FullName)!;
        }

        public static string GetActiveProjectName()
        {
            var project = GetActiveProject();
            if (project == null)
            {
                throw new Exception("No active project found.");
            }

            return project.Name;
        }

        private static void FixPath(string projectName, ref string documentPath)
        {
            // convert to absolute path unless already is so
            if (!Path.IsPathRooted(documentPath))
            {
                documentPath = Path.Combine(GetActiveProjectPath(), documentPath);
            }
        }

        public static void CreateFolder(string projectName, string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var project = GetProjectsInSolution().FirstOrDefault(x => x.Name == projectName);
            if (project == null)
            {
                throw new Exception($"No project {projectName} found.");
            }

            project.ProjectItems.AddFromDirectory(folderPath);
        }

        public static void CreateDocument(string projectName, string documentPath)
        {
            var project = GetProjectsInSolution().FirstOrDefault(x => x.Name == projectName);
            if (project == null)
            {
                throw new Exception($"No project {projectName} found.");
            }

            if (!File.Exists(documentPath))
            {
                // create folder if doesn't exist
                var folder = Path.GetDirectoryName(documentPath);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder!);
                }

                File.Create(documentPath).Close();
                project.ProjectItems.AddFromFile(documentPath);
            }

            OpenDocument(documentPath);
        }

        public static void DeleteDocument(string projectName, string documentPath)
        {
            var project = GetProjectsInSolution().FirstOrDefault(x => x.Name == projectName);
            if (project == null)
            {
                throw new Exception($"No project {projectName} found.");
            }

            var projectItem = project.ProjectItems.Item(documentPath);
            projectItem?.Remove();

            if (File.Exists(documentPath))
            {
                File.Delete(documentPath);
            }
        }

        public static bool DocumentExists(string documentPath)
        {
            return File.Exists(documentPath);
        }

        public static void OpenDocument(string documentPath)
        {
            if (!DocumentExists(documentPath))
            {
                throw new Exception("File does not exist.");
            }

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            dte.ItemOperations.OpenFile(documentPath);
        }

        public static void CloseDocument(string documentPath)
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            var document = dte.Documents.Item(documentPath);
            document.Close();
        }

        public static void SaveDocument(string? documentPath = null)
        {
            if (documentPath == null)
            {
                documentPath = GetActiveDocumentPath();
            }

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            Assumes.Present(dte);
            var item = dte.Solution.FindProjectItem(documentPath);
            if (item is { Document: not null })
            {
                var document = item.Document;
                document.Save();
            }
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
                    ["description"] = errorItem.Description,
                    ["error_level"] = errorItem.ErrorLevel.ToString(),
                    ["document_path"] = errorItem.FileName,
                    ["line"] = errorItem.Line,
                    ["column"] = errorItem.Column
                };

                errorArray.Add(errorObject);
            }

            return errorArray;
        }
        #endregion
    }
}