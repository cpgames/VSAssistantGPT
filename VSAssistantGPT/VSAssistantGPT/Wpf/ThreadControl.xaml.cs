using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using cpGames.VSA.ViewModel;

namespace cpGames.VSA.Wpf
{
    /// <summary>
    ///     Interaction logic for ThreadControl.xaml
    /// </summary>
    public partial class ThreadControl : UserControl
    {
        #region Properties
        public ThreadViewModel? ViewModel
        {
            get => DataContext as ThreadViewModel;
            set => DataContext = value;
        }
        #endregion

        #region Constructors
        public ThreadControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        #endregion

        #region Methods
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
            MessageText.TextChanged += (s, e) => UpdatePlaceholderVisibility();
            MessageText.GotFocus += (s, e) => UpdatePlaceholderVisibility();
            MessageText.LostFocus += (s, e) => UpdatePlaceholderVisibility();
            MessageText.Focus();
        }

        private void UpdatePlaceholderVisibility()
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(MessageText.Text) && !MessageText.IsFocused
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private async void SubmitClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null ||
                string.IsNullOrWhiteSpace(MessageText.Text))
            {
                return;
            }

            var text = MessageText.Text;
            try
            {
                MessageText.Text = "";
                SubmitButton.IsEnabled = false;
                await ViewModel.PostMessageAsync(text);
                SubmitButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                SubmitButton.IsEnabled = true;
                MessageText.Text = text;
            }
        }

        private void MessageTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                SubmitClicked(sender, e);
                e.Handled = true;
            }
        }

        private async void SelectAssistantClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/VSA;component/generic.xaml", UriKind.RelativeOrAbsolute)
            };
            var menuTemplate = resourceDictionary["SimpleMenuTemplate"] as ControlTemplate;
            if (menuTemplate == null)
            {
                return;
            }

            var itemTemplate = resourceDictionary["SimpleMenuItemTemplate"] as ControlTemplate;
            if (itemTemplate == null)
            {
                return;
            }

            if (ProjectUtils.ActiveProject.Assistants.Count == 0)
            {
                await ProjectUtils.ActiveProject.LoadAssistantsAsync();
            }

            var contextMenu = new ContextMenu
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                Template = menuTemplate
            };
            foreach (var assistant in ProjectUtils.ActiveProject.Assistants)
            {
                var menuItem = new MenuItem
                {
                    Header = assistant.Name,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Template = itemTemplate
                };
                menuItem.Click += (s, a) => { ViewModel.Assistant = assistant; };
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.IsOpen = true;
        }

        private async void RefreshThreadClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(ViewModel.Id))
            {
                await ViewModel.DeleteAsync();
            }

            await ViewModel.CreateAsync();
        }
        #endregion
    }
}