using System.Windows;
using System.Windows.Controls;
using cpGames.VSA.ViewModel;

namespace cpGames.VSA.Wpf
{
    /// <summary>
    /// Interaction logic for VectorStoreControl.xaml
    /// </summary>
    public partial class VectorStoreControl : UserControl
    {
        #region Properties
        public VectorStoreViewModel? ViewModel
        {
            get => DataContext as VectorStoreViewModel;
            set => DataContext = value;
        }
        #endregion

        #region Constructors
        public VectorStoreControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        private void SelectAllVectorStoresChecked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            foreach (var file in ViewModel.Files)
            {
                file.Selected = true;
            }
        }

        private void SelectAllVectorStoresUnchecked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            foreach (var file in ViewModel.Files)
            {
                file.Selected = false;
            }
        }

        private async void LoadFilesClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            await ViewModel.LoadFilesAsync();
        }

        private async void SyncFilesClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            await ViewModel.SyncFilesAsync();
        }

        private async void DeleteClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            await ViewModel.DeleteAsync();
        }

        private async void VectorStoreExpanded(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            if (ViewModel.Files.Count == 0)
            {
                await ViewModel.LoadFilesAsync();
            }
        }
        #endregion
    }
}