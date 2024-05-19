using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace cpGames.VSA
{
    [ComVisible(true)]
    public class OptionsPage : DialogPage
    {
        #region Properties
        [Category("General")]
        [DisplayName("API Key")]
        [Description("Your OpenAI API Key.")]
        public string APIKey { get; set; } = "";

        [Category("Assistant")]
        [DisplayName("Assistant Description")]
        [Description("Assistant description guides AI to generate better response.")]
        public string AssistantDescription { get; set; } = "You are a programming assistant named Don";
        #endregion
    }

    public static class OptionPageHelpers
    {
        #region Methods
        public static OptionsPage? GetOptionsPage()
        {
            ThreadHelper.ThrowIfNotOnUIThread("Needs to be called on the UI thread.");
            if (Package.GetGlobalService(typeof(SVsShell)) is IVsShell shell)
            {
                ErrorHandler.ThrowOnFailure(shell.LoadPackage(ref PackageGuids.VSAGuid, out var package));
                if (package is VSAssistantGPTPackage donGPTPackage)
                {
                    return donGPTPackage.GetDialogPage(typeof(OptionsPage)) as OptionsPage;
                }
            }
            return null;
        }
        #endregion
    }
}