using System.Windows;
using System.Windows.Controls;
using cpGames.VSA.ViewModel;

namespace cpGames.VSA.Wpf
{
    /// <summary>
    /// Interaction logic for ToolEntryControl.xaml
    /// </summary>
    public partial class ToolEntryControl : UserControl
    {
        #region Properties
        public ToolEntryViewModel? ViewModel
        {
            get => DataContext as ToolEntryViewModel;
            set => DataContext = value;
        }
        #endregion

        #region Constructors
        public ToolEntryControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        private void RemoveClicked(object sender, RoutedEventArgs e)
        {
            ViewModel?.Remove();
        }
        #endregion
    }
}