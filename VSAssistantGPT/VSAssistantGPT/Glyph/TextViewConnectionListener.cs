using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace cpGames.VSA
{
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class TextViewConnectionListener : IWpfTextViewConnectionListener
    {
        #region IWpfTextViewConnectionListener Members
        public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            foreach (var buffer in subjectBuffers)
            {
                if (!buffer.Properties.ContainsProperty(typeof(VSATagger)))
                {
                    var tagger = new VSATagger(textView);
                    buffer.Properties.AddProperty(typeof(VSATagger), tagger);
                }
            }
        }

        public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            foreach (var buffer in subjectBuffers)
            {
                if (buffer.Properties.ContainsProperty(typeof(VSATagger)))
                {
                    buffer.Properties.RemoveProperty(typeof(VSATagger));
                }
            }
        }
        #endregion
    }
}