using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace cpGames.VSA
{
    [Command(PackageIds.ShowHelperCommand)]
    internal sealed class ShowHelperCommand : BaseCommand<ShowHelperCommand>
    {
        #region Properties
        public static ShowHelperCommand? Instance { get; private set; }
        #endregion

        #region Methods
        protected override Task InitializeCompletedAsync()
        {
            Instance = this;
            return base.InitializeCompletedAsync();
        }

        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            // Command execution logic can go here, if needed.
            return Task.CompletedTask;
        }

        public void HookIntoTextView(IWpfTextView textView)
        {
            textView.Selection.SelectionChanged += (sender, args) =>
            {
                var selectedText = textView.Selection.SelectedSpans.FirstOrDefault().GetText();
                if (!string.IsNullOrEmpty(selectedText))
                {
                    ShowIconNearText(textView, textView.Selection.Start.Position);
                }
            };
        }

        private void ShowIconNearText(IWpfTextView textView, int position)
        {
            var textViewLine = textView.GetTextViewLineContainingBufferPosition(new SnapshotPoint(textView.TextSnapshot, position));
            var top = textViewLine.Top;
            var left = textViewLine.Left;

            var icon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/VSA;component/Resources/icons/Bot.png")),
                Width = 16, // Set the desired width for your icon
                Height = 16 // Set the desired height for your icon
            };

            Canvas.SetTop(icon, top);
            Canvas.SetLeft(icon, left);

            var adornmentLayer = textView.GetAdornmentLayer("IconLayer");
            adornmentLayer.AddAdornment(
                AdornmentPositioningBehavior.ViewportRelative,
                null,
                null,
                icon,
                null);
        }
        #endregion
    }
}