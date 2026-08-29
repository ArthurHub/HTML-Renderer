using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Builds the detached row clones <see cref="CssBox.RepeatedHeaderRows"/> holds - css-tables-3
    /// 6.2's repeated &lt;thead&gt; content, one clone per continuation page a table's body spans.
    /// </summary>
    /// <remarks>
    /// Unlike PeachPDF's proxy-based approach (a shared source subtree re-emitted at each page's
    /// position purely at the fragment-tree level), this clones real, laid-out <see cref="CssBox"/>
    /// instances. That's a deliberate simplification for this port: it makes the repeat visible
    /// through both the existing scroll-offset PDF pipeline and the new fragment tree without
    /// teaching two different rendering paths about "one source, several positions" - at the cost of
    /// only reproducing what a clone can cheaply carry over (a header's own decoration and words;
    /// multi-line per-box decoration rectangles inside a header cell are not reproduced, since that
    /// needs cloning CssLineBox instances too - an accepted gap for the common single-line-header
    /// case this feature targets).
    /// </remarks>
    internal static class TableHeaderRepeat
    {
        /// <summary>
        /// Clones <paramref name="source"/> (a &lt;thead&gt; row, recursively with its cells and their
        /// content) and shifts the clone so the row's rendered top - <paramref name="sourceRenderedTop"/>,
        /// the caller's own reference, since a &lt;tr&gt; box's own Location is never assigned by table
        /// layout (only its cells' is - see <see cref="Dom.CssLayoutEngineTable"/>'s row loop) - lands
        /// at <paramref name="targetTop"/>. The clone is fully detached - not part of any box's
        /// <see cref="CssBox.Boxes"/> - so re-running table layout can never mistake it for real content.
        /// </summary>
        internal static CssBox CloneAndPosition(CssBox source, double sourceRenderedTop, double targetTop)
        {
            var clone = CloneSubtree(source, null);
            var delta = targetTop - sourceRenderedTop;
            if (delta != 0)
                clone.OffsetTop(delta);
            return clone;
        }

        private static CssBox CloneSubtree(CssBox source, CssBox newParent)
        {
            var clone = new CssBox(newParent, source.HtmlTag);
            clone.InheritStyle(source, everything: true);
            clone.HtmlContainer = source.HtmlContainer;
            clone.Location = source.Location;
            clone.Size = source.Size;
            clone.ActualBottom = source.ActualBottom;
            clone.ActualRight = source.ActualRight;

            if (source.Words.Count > 0)
            {
                clone.Text = source.Text;
                clone.ParseToWords();

                // Reuses the source's already-measured word geometry rather than re-measuring - the
                // clone's tokenization matches the source's own (same Text, same ParseToWords), so a
                // positional pairing is safe here.
                var count = clone.Words.Count < source.Words.Count ? clone.Words.Count : source.Words.Count;
                for (var i = 0; i < count; i++)
                {
                    clone.Words[i].Left = source.Words[i].Left;
                    clone.Words[i].Top = source.Words[i].Top;
                    clone.Words[i].Width = source.Words[i].Width;
                    clone.Words[i].Height = source.Words[i].Height;
                }
            }

            foreach (var child in source.Boxes)
            {
                CloneSubtree(child, clone);
            }

            return clone;
        }
    }
}
