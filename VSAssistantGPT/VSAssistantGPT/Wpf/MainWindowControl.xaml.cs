using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace cpGames.VSA.Wpf
{
    public partial class MainWindowControl : UserControl
    {
        #region Constructors
        public MainWindowControl()
        {
            InitializeComponent();
            DisplayNoProjectLoaded();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        #endregion

        #region Events
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ProjectUtils.onProjectLoaded += DisplayProject;
            ProjectUtils.onProjectUnloaded += DisplayNoProjectLoaded;

            if (DTEUtils.IsSolutionOpen())
            {
                ProjectUtils.CreateOrLoadProject();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ProjectUtils.onProjectLoaded -= DisplayProject;
            ProjectUtils.onProjectUnloaded -= DisplayNoProjectLoaded;
        }
        #endregion

        #region Methods
        private void DisplayProject()
        {
            var projectControl = new ProjectEntryControl();
            contentControl.Content = projectControl;
            createProjectButton.Visibility = Visibility.Collapsed;
            saveProjectButton.Visibility = Visibility.Visible;
        }

        private void DisplayNoProjectLoaded()
        {
            contentControl.Content = new Label
            {
                Content = "No Project Loaded",
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            createProjectButton.Visibility = Visibility.Visible;
            saveProjectButton.Visibility = Visibility.Collapsed;
        }

        private void CreateProjectClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                ProjectUtils.CreateOrLoadProject();
            }
            catch (Exception ex) // Generic exception catch, if you expect other kinds of exceptions
            {
                MessageBox.Show(
                    $"An unexpected error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SaveProjectClicked(object sender, RoutedEventArgs e)
        {
            if (ProjectUtils.ActiveProject == null)
            {
                return;
            }
            ProjectUtils.ActiveProject.Save();
        }
        #endregion
    }
}