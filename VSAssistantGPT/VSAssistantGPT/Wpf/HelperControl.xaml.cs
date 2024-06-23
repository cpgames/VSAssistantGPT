using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using cpGames.VSA.ViewModel;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace cpGames.VSA.Wpf
{
    /// <summary>
    ///     Interaction logic for HelperControl.xaml
    /// </summary>
    public partial class HelperControl : UserControl
    {
        #region Fields
        private readonly DialogWindow _dialogWindow;
        private bool _isDragging;
        private Point _startPoint;
        private bool _isClosing;
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
                        if (!_isClosing)
                        {
                            _dialogWindow.Close();
                        }
                    };
                    //  find "MessageText" textbox in Content and set focus
                    // var messageText = FindName("MessageText") as TextBox;
                    // messageText?.Focus();
                }).ConfigureAwait(false);
        }

        private void DragButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _startPoint = _dialogWindow.PointToScreen(e.GetPosition(_dialogWindow));
            Mouse.Capture((UIElement)sender);
        }

        private void DragButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            Mouse.Capture(null);
        }

        private void DragButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var currentPosition = _dialogWindow.PointToScreen(e.GetPosition(_dialogWindow));
                var offsetX = currentPosition.X - _startPoint.X;
                var offsetY = currentPosition.Y - _startPoint.Y;

                _dialogWindow.Left += offsetX;
                _dialogWindow.Top += offsetY;

                // Update start point for the next move
                _startPoint = currentPosition;
            }
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            _isClosing = true;
            _dialogWindow.Close();
        }
        #endregion
    }
}