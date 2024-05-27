using System;
using System.IO;
using System.Reflection;
using cpGames.VSA.ViewModel;
using Newtonsoft.Json;

namespace cpGames.VSA
{
    public static class ProjectUtils
    {
        #region Fields
        public const string PROJ_EXTENSION = ".dproj";

        private static readonly ProjectViewModel _activeProject;
        #endregion

        #region Properties
        public static ProjectViewModel ActiveProject => _activeProject;
        #endregion

        #region Constructors
        static ProjectUtils()
        {
            var settingsPath = Path.Combine(Utils.GetOrCreateAppDir(), "settings.json");
            if (!File.Exists(settingsPath))
            {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var defaultSettingsPath = Path.Combine(Path.GetDirectoryName(assemblyPath)!, "Resources", "settings.json");
                File.Copy(defaultSettingsPath, settingsPath);
            }
            _activeProject = CreateOrLoadProject();
        }
        #endregion

        #region Methods
        private static ProjectViewModel CreateOrLoadProject()
        {
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
            var projectViewModel = new ProjectViewModel(project);
            if (shouldSave)
            {
                SaveProject();
            }
            return projectViewModel;
        }

        public static void SaveProject()
        {
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
}