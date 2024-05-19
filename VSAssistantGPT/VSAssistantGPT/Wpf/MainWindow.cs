using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;

namespace cpGames.VSA.Wpf
{
    public class MainWindow : BaseToolWindow<MainWindow>
    {
        #region Nested type: Pane
        [Guid("FBF933D5-C685-4357-B334-55E8179B36AD")]
        public class Pane : ToolWindowPane
        {
            #region Constructors
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
            #endregion
        }
        #endregion

        #region Properties
        public override Type PaneType => typeof(Pane);
        #endregion

        #region Methods
        public override string GetTitle(int toolWindowId)
        {
            return "VS Assistant";
        }

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new MainWindowControl());
        }
        #endregion
    }
}