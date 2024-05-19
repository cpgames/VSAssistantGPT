using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace cpGames.VSA
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(VSATag))]
    internal class VSATaggerProvider : ITaggerProvider
    {
        #region ITaggerProvider Members
        public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (!buffer.Properties.TryGetProperty(typeof(VSATagger), out VSATagger tagger))
            {
                // Fallback or error handling if needed
                return null;
            }

            return tagger as ITagger<T>;
        }
        #endregion
    }
}