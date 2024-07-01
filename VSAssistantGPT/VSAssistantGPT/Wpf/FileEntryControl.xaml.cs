using System.Windows;
using System.Windows.Controls;
using cpGames.VSA.ViewModel;

namespace cpGames.VSA.Wpf
{
    /// <summary>
    ///     Interaction logic for FileEntryControl.xaml
    /// </summary>
    public partial class FileEntryControl : UserControl
    {
        #region Properties
        public FileViewModel? ViewModel
        {
            get => DataContext as FileViewModel;
            set => DataContext = value;
        }
        #endregion

        #region Constructors
        public FileEntryControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        private async void DeleteClicked(object sender, RoutedEventArgs e)
        {
            await ViewModel?.DeleteAsync();
        }

        private async void SyncClicked(object sender, RoutedEventArgs e)
        {
            await ViewModel?.SyncAsync();
        }
        #endregion
    }
}