using System.Windows;
using System.Windows.Controls;

namespace cpGames.VSA.Wpf
{
    public partial class MainWindowControl : UserControl
    {
        #region Constructors
        public MainWindowControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }
        #endregion

        #region Events
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ProjectUtils.CreateOrLoadProject();
            var projectControl = new ProjectEntryControl();
            contentControl.Content = projectControl;
        }
        #endregion
    }
}