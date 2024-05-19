using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using cpGames.VSA.Wpf;
using Microsoft.VisualStudio.Shell;

namespace cpGames.VSA
{
    [Command(PackageIds.ShowVSACommand)]
    internal sealed class ShowVSACommand : BaseCommand<ShowVSACommand>
    {
        #region Methods
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return MainWindow.ShowAsync();
        }
        #endregion
    }
}