using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Inline-flow page-break corrections, applied the same way as <see cref="BlockFragmentation"/>:
    /// as local shifts to lines <see cref="CssLayoutEngine.CreateLineBoxes"/> has already computed, not
    /// via a resumable re-entry into word measurement/line breaking. A line box is monolithic
    /// (css-break-3 4.1) and never straddles a page boundary; where the whole run of already-laid-out
    /// lines from the last break point would otherwise have too few lines before it (<c>orphans</c>) or
    /// leave too few after (<c>widows</c>), the break point moves instead of the line count.
    /// </summary>
    internal static class InlineFragmentation
    {
        private const double Epsilon = 0.01;

        /// <summary>
        /// Called right after <see cref="CssLayoutEngine.CreateLineBoxes"/> finishes for
        /// <paramref name="blockBox"/>: pushes any line that straddles a page boundary - and, honoring
        /// <c>orphans</c>/<c>widows</c>, the lines around it - down to the next page's content top, then
        /// updates <see cref="CssBoxProperties.ActualBottom"/> to match.
        /// </summary>
        internal static void ApplyLineBreaking(CssBox blockBox)
        {
            var container = blockBox.HtmlContainer;
            if (container == null || !container.HasRealPageGrid)
                return;

            var lines = blockBox.LineBoxes;
            if (lines.Count == 0)
                return;

            var orphans = blockBox.ActualOrphans;
            var widows = blockBox.ActualWidows;

            var delta = 0.0;
            // Index of the first line of the current "page run" within this box - what orphans/widows
            // are counted against.
            var pageStart = 0;

            for (var i = 0; i < lines.Count; i++)
            {
                if (delta != 0)
                    lines[i].ShiftLine(delta);

                var top = lines[i].LineTop;
                var bottom = lines[i].LineBottom;
                if (bottom <= top)
                    continue;

                // Bottom-edge convention: a bottom landing exactly on a boundary belongs to the band above it.
                if (container.PageIndexOf(System.Math.Max(top, bottom - Epsilon)) <= container.PageIndexOf(top))
                    continue; // this line doesn't straddle - nothing to do

                var breakIndex = i;

                // Orphans: at least `orphans` lines must remain on the page before the break.
                var linesBefore = breakIndex - pageStart;
                if (linesBefore > 0 && linesBefore < orphans)
                    breakIndex = pageStart;

                // Widows: at least `widows` lines must remain after the break, in total for this box.
                var linesAfter = lines.Count - breakIndex;
                if (linesAfter > 0 && linesAfter < widows && lines.Count - widows >= pageStart)
                    breakIndex = System.Math.Min(breakIndex, lines.Count - widows);

                var target = container.PageTopOf(container.PageIndexOf(lines[breakIndex].LineTop) + 1);
                var shift = target - lines[breakIndex].LineTop;

                if (shift > 0)
                {
                    for (var j = breakIndex; j <= i; j++)
                    {
                        lines[j].ShiftLine(shift);
                    }
                    delta += shift;
                }

                pageStart = breakIndex;
            }

            var maxBottom = 0.0;
            foreach (var line in lines)
            {
                maxBottom = System.Math.Max(maxBottom, line.LineBottom);
            }

            if (maxBottom > 0)
            {
                blockBox.ActualBottom = maxBottom + blockBox.ActualPaddingBottom + blockBox.ActualBorderBottomWidth;
            }
        }
    }
}
