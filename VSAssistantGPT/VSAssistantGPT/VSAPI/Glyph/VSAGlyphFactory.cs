using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using cpGames.VSA.Wpf;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace cpGames.VSA
{
    internal class VSAGlyphFactory : IGlyphFactory
    {
        #region IGlyphFactory Members
        public UIElement? GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            if (tag is VSATag)
            {
                var image = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/VSA;component/Resources/icons/bot.png")),
                    Width = 16,
                    Height = 16,
                    Margin = new Thickness(2),
                    Cursor = Cursors.Hand
                };

                image.MouseLeftButtonDown += ImageMouseLeftButtonDown;
                return image;
            }

            return null;
        }
        #endregion

        #region Methods
        private void ImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                var relativePosition = e.GetPosition(element);
                var screenPosition = element.PointToScreen(relativePosition);
                ShowVSAInput(screenPosition);
                e.Handled = true;
            }
        }

        private void ShowVSAInput(Point position)
        {
            if (ProjectUtils.ActiveProject == null)
            {
                return;
            }
            var dialog = new DialogWindow();
            var helperControl = new HelperControl(dialog)
            {
                ViewModel = ProjectUtils.ActiveProject
            };
            dialog.HasDialogFrame = false;
            dialog.WindowStyle = WindowStyle.None;
            dialog.ResizeMode = ResizeMode.NoResize;
            dialog.Content = helperControl;
            dialog.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            dialog.AllowsTransparency = true;
            dialog.Width = 300;
            dialog.Height = 200;
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;
            dialog.Left = position.X;
            dialog.Top = position.Y;
            dialog.Opacity = 0.7;
            dialog.ShowModal();
        }
        #endregion
    }
}