using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace cpGames.VSA
{
    public class OutputWindowHelper
    {
        private static readonly Lazy<OutputWindowHelper> _instance = new(() => new OutputWindowHelper());
        private IVsOutputWindowPane? _customPane;
        private Guid _paneGuid = new("3A9E362B-D6D4-4B68-B448-DCAD0B1B8760");

        public static OutputWindowHelper Instance => _instance.Value;

        private OutputWindowHelper()
        {
            InitializePaneAsync().ConfigureAwait(false);
        }

        private async Task InitializePaneAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            if (outputWindow == null) return;

            if (outputWindow.GetPane(ref _paneGuid, out _customPane) != VSConstants.S_OK)
            {
                outputWindow.CreatePane(ref _paneGuid, "AssistantGPT", 1, 1);
                outputWindow.GetPane(ref _paneGuid, out _customPane);
            }
        }

        private async Task WriteToOutputAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _customPane?.OutputStringThreadSafe(message + Environment.NewLine);
        }

        private void WriteToOutput(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _customPane?.OutputStringThreadSafe(message + Environment.NewLine);
        }

        public static async Task LogInfoAsync(string context, string message)
        {
            await Instance.WriteToOutputAsync($"[INFO] - {context}: {message}");
        }

        public static async Task LogWarningAsync(string context, string message)
        {
            await Instance.WriteToOutputAsync($"[WARNING] - {context}: {message}");
        }

        public static async Task LogErrorAsync(string context, string message)
        {
            await Instance.WriteToOutputAsync($"[ERROR] - {context}: {message}");
        }

        public static void LogInfo(string context, string message)
        {
            Instance.WriteToOutput($"[INFO] - {context}: {message}");
        }

        public static void LogWarning(string context, string message)
        {
            Instance.WriteToOutput($"[WARNING] - {context}: {message}");
        }

        public static void LogError(string context, string message)
        {
            Instance.WriteToOutput($"[ERROR] - {context}: {message}");
        }
    }
}
