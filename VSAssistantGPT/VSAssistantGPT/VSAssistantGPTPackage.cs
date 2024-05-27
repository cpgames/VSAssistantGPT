using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using cpGames.VSA.Wpf;
using Microsoft.VisualStudio.Shell;

namespace cpGames.VSA
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(MainWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.VSAGuidString)]
    [ProvideOptionPage(
        typeof(OptionsPage),
        "VSA",
        "Settings",
        0,
        0,
        true)]
    public sealed class VSAssistantGPTPackage : ToolkitPackage
    {
        #region Methods
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            MainWindow.Initialize(this);

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ShowVSACommand.InitializeAsync(this);
        }
        #endregion
    }
}