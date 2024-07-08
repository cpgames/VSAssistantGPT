using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace cpGames.VSA
{
    [Export(typeof(IGlyphFactoryProvider))]
    [Name("VSAGlyphProvider")]
    [Order(After = "VsTextMarker")]
    [ContentType("code")]
    [TagType(typeof(VSATag))]
    internal class VSAGlyphFactoryProvider : IGlyphFactoryProvider
    {
        #region IGlyphFactoryProvider Members
        public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
        {
            return new VSAGlyphFactory();
        }
        #endregion
    }
}