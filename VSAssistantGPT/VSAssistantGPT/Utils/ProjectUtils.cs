using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using cpGames.VSA.ViewModel;
using Newtonsoft.Json;

namespace cpGames.VSA
{
    public static class ProjectUtils
    {
        #region Fields
        public const string PROJ_EXTENSION = ".dproj";

        private static ProjectViewModel? _activeProject;

        public static Action? onProjectLoaded;
        public static Action? onProjectUnloaded;
        #endregion

        #region Properties
        public static ProjectViewModel? ActiveProject
        {
            get => _activeProject;
            set
            {
                if (_activeProject == value)
                {
                    return;
                }
                _activeProject = value;
                if (_activeProject != null)
                {
                    OutputWindowHelper.LogInfo("Project", $"Loaded project: {_activeProject.Name}");
                    onProjectLoaded?.Invoke();
                }
                else
                {
                    onProjectUnloaded?.Invoke();
                }
            }
        }
        #endregion

        #region Methods
        public static string GetProjectFilePath(ProjectModel project)
        {
            var solutionPath = DTEUtils.GetSolutionFolderPath();
            return Path.Combine(solutionPath, $"{project.name}.dproj");
        }

        public static string GetProjectFilePath()
        {
            var solutionPath = DTEUtils.GetSolutionFolderPath();
            if (string.IsNullOrEmpty(solutionPath))
            {
                throw new InvalidOperationException("Solution path is null or empty.");
            }
            var directoryInfo = new DirectoryInfo(solutionPath);
            var projectFile = directoryInfo.GetFiles("*.dproj").FirstOrDefault();
            if (projectFile == null)
            {
                throw new InvalidOperationException("Project file not found.");
            }
            return projectFile.FullName;
        }

        public static ProjectModel LoadProject()
        {
            var solutionPath = DTEUtils.GetSolutionFolderPath();
            var directoryInfo = new DirectoryInfo(solutionPath);
            var projectFile = directoryInfo.GetFiles("*.dproj").FirstOrDefault();
            if (projectFile == null)
            {
                throw new InvalidOperationException("Project file not found.");
            }

            using (var file = File.OpenText(projectFile.FullName))
            {
                var serializer = new JsonSerializer();
                var model = (ProjectModel?)serializer.Deserialize(file, typeof(ProjectModel));
                if (model == null)
                {
                    throw new InvalidOperationException("Failed to deserialize project model.");
                }
                return model;
            }
        }

        public static void CreateOrLoadProject()
        {
            if (ActiveProject != null)
            {
                return;
            }
            var solutionPath = DTEUtils.GetSolutionFolderPath();
            var directoryInfo = new DirectoryInfo(solutionPath);
            var projectFile = directoryInfo.GetFiles("*.dproj").FirstOrDefault();
            ProjectModel? project;
            if (projectFile == null)
            {
                project = new ProjectModel();
                SaveProject(project);
            }
            else
            {
                project = LoadProject();
            }
            if (directoryInfo.GetFiles("AssistantTools.py").Length == 0)
            {
                // copy from application bin path
                var binPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
                File.Copy(Path.Combine(binPath, "Resources/AssistantTools.py"), Path.Combine(DTEUtils.GetSolutionFolderPath(), "AssistantTools.py"));
            }
            ActiveProject = new ProjectViewModel(project);
        }

        public static void SaveProject(ProjectModel project)
        {
            var projectFilePath = GetProjectFilePath(project);

            // Delete existing project file if a different name is detected
            var solutionPath = DTEUtils.GetSolutionFolderPath();
            var existingFiles = Directory.GetFiles(solutionPath, "*.dproj");
            foreach (var file in existingFiles)
            {
                if (Path.GetFileName(file) != Path.GetFileName(projectFilePath))
                {
                    File.Delete(file);
                }
            }

            // Save the current project
            using (var file = File.CreateText(projectFilePath))
            {
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(file, project);
            }
        }
        #endregion
    }

    public class StatusToColorConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            if (value is TaskStatus status)
            {
                switch (status)
                {
                    case TaskStatus.NotStarted:
                        return Brushes.Cyan;
                    case TaskStatus.InProgress:
                        return Brushes.Yellow;
                    case TaskStatus.Completed:
                        return Brushes.GreenYellow;
                    default:
                        return Brushes.Red;
                }
            }
            return Brushes.White;
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class AssistantNameToColorConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            if (value is string assistantName &&
                ProjectUtils.ActiveProject != null)
            {
                if (ProjectUtils.ActiveProject.SelectedAssistant == assistantName)
                {
                    return Brushes.White;
                }
            }
            return Brushes.Black;
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}