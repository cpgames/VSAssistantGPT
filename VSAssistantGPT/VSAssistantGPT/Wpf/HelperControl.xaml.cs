using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using cpGames.VSA.ViewModel;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace cpGames.VSA.Wpf
{
    /// <summary>
    /// Interaction logic for HelperControl.xaml
    /// </summary>
    public partial class HelperControl : UserControl
    {
        #region Fields
        private readonly DialogWindow _dialogWindow;
        #endregion

        #region Properties
        public ProjectViewModel? ViewModel
        {
            get => DataContext as ProjectViewModel;
            set => DataContext = value;
        }
        #endregion

        #region Constructors
        public HelperControl(DialogWindow dialogWindow)
        {
            _dialogWindow = dialogWindow;
            InitializeComponent();
            Loaded += HelperControl_Loaded;
        }
        #endregion

        #region Methods
        private void HelperControl_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterCloseOnDeactivate();
        }

        private void RegisterCloseOnDeactivate()
        {
            Task.Run(
                async () =>
                {
                    Thread.Sleep(100);
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    _dialogWindow.Activate();
                    _dialogWindow.Deactivated += (s, e) =>
                    {
                        _dialogWindow.Close();
                    };
                    //  find "MessageText" textbox in Content and set focus
                    // var messageText = FindName("MessageText") as TextBox;
                    // messageText?.Focus();
                }).ConfigureAwait(false);
        }
        #endregion
    }
}