using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Builds the detached row clones <see cref="CssBox.RepeatedHeaderRows"/> holds, the &lt;thead&gt;
    /// rows repeated on every page a table continues on (CSS 2.1 §17.2).
    /// </summary>
    /// <remarks>
    /// A clone carries the header's decoration and words, not its line boxes, so the per-line
    /// decoration rectangles of a multi-line header cell aren't repeated.
    /// </remarks>
    internal static class TableHeaderRepeat
    {
        /// <summary>
        /// Clones <paramref name="source"/> (a &lt;thead&gt; row with its cells and their content) and
        /// shifts the clone from <paramref name="sourceRenderedTop"/> to <paramref name="targetTop"/>. The
        /// caller passes the row's top because table layout only positions the cells, not the
        /// &lt;tr&gt; box. The clone isn't in any box's <see cref="CssBox.Boxes"/>, so table layout never
        /// sees it.
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

                // Same text, same words, so the source's measured geometry pairs up by index.
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
