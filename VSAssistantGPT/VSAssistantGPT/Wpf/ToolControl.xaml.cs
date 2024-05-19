using System.Windows;
using System.Windows.Controls;
using cpGames.VSA.ViewModel;

namespace cpGames.VSA.Wpf
{
    /// <summary>
    /// Interaction logic for ToolControl.xaml
    /// </summary>
    public partial class ToolControl : UserControl
    {
        #region Properties
        public ToolViewModel? ViewModel
        {
            get => DataContext as ToolViewModel;
            set => DataContext = value;
        }
        #endregion

        #region Constructors
        public ToolControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        private void RemoveToolClicked(object sender, RoutedEventArgs e)
        {
            ViewModel?.Remove();
        }

        private void SaveToolClicked(object sender, RoutedEventArgs e)
        {
            ViewModel?.Save();
        }
        #endregion
    }
}