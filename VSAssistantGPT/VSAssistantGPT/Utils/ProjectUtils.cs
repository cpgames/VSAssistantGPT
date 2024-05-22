using System;
using System.Globalization;
using System.IO;
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
        public static void CreateOrLoadProject()
        {
            if (ActiveProject != null)
            {
                return;
            }
            var settingsPath = Path.Combine(Utils.GetOrCreateAppDir(), "settings.json");
            ProjectModel? project;
            var shouldSave = false;
            if (!File.Exists(settingsPath))
            {
                project = new ProjectModel();
                shouldSave = true;
            }
            else
            {
                using (var file = File.OpenText(settingsPath))
                {
                    var serializer = new JsonSerializer();
                    var model = (ProjectModel?)serializer.Deserialize(file, typeof(ProjectModel));
                    if (model == null)
                    {
                        throw new InvalidOperationException("Failed to deserialize settings.");
                    }
                    project = model;
                }
            }
            ActiveProject = new ProjectViewModel(project);
            if (shouldSave)
            {
                SaveProject();
            }
        }

        public static void SaveProject()
        {
            if (ActiveProject == null)
            {
                throw new InvalidOperationException("No active project to save.");
            }
            var settingsPath = Path.Combine(Utils.GetOrCreateAppDir(), "settings.json");
            using (var file = File.CreateText(settingsPath))
            {
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(file, ActiveProject.Model);
            }
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