using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace cpGames.VSA
{
    internal class VSATagger : ITagger<VSATag>
    {
        #region Properties
        public static IWpfTextView? View { get; private set; }
        public static ITextBuffer? Buffer { get; private set; }
        #endregion

        #region Constructors
        public VSATagger(IWpfTextView view)
        {
            View = view;
            Buffer = view.TextBuffer;
            View.Selection.SelectionChanged += OnSelectionChanged;
        }
        #endregion

        #region Events
        private void OnSelectionChanged(object sender, EventArgs e)
        {
            var spans = new List<SnapshotSpan>();
            if (!View!.Selection.IsEmpty)
            {
                var selectedSpans = View.Selection.SelectedSpans;
                foreach (var span in selectedSpans)
                {
                    spans.Add(new SnapshotSpan(Buffer!.CurrentSnapshot, span));
                }
            }

            // Notify the change of tags
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(Buffer!.CurrentSnapshot, 0, Buffer.CurrentSnapshot.Length)));
        }
        #endregion

        #region ITagger<VSATag> Members
        public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

        public IEnumerable<ITagSpan<VSATag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (View!.Selection.IsEmpty)
            {
                yield break;
            }

            // Obtain the start point of the selection in the buffer
            var startOfSelection = View.Selection.Start.Position;

            // Check each span to find the one that contains the start of the selection
            foreach (var span in spans)
            {
                if (span.Start <= startOfSelection && span.End >= startOfSelection)
                {
                    // If this span contains the start of the selection, yield a tag for it
                    yield return new TagSpan<VSATag>(span, new VSATag());
                    break;  // Only tag the first matching span
                }
            }
        }
        #endregion

        #region Methods
        public void RemoveTagger()
        {
            View!.Selection.SelectionChanged -= OnSelectionChanged;
            View = null;
            Buffer = null;
        }
        #endregion
    }
}