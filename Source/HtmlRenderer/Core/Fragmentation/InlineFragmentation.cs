using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Page breaks between the line boxes of a block container (CSS 2.1 §13.3.3, css-break-3 §5.4). Lines
    /// already laid out by <see cref="CssLayoutEngine.CreateLineBoxes"/> are shifted, a line never straddles
    /// a page boundary and <c>orphans</c>/<c>widows</c> move the break instead of the lines.
    /// </summary>
    internal static class InlineFragmentation
    {
        /// <summary>
        /// Moves the lines of <paramref name="blockBox"/> that start a new page down to that page's top and
        /// updates <see cref="CssBoxProperties.ActualBottom"/> to match.
        /// </summary>
        /// <remarks>
        /// Phase 1 decides all breaks from the lines' unshifted positions. A run's height doesn't change when
        /// it's shifted as a whole, so whether a run fits on a page doesn't depend on where it lands, and
        /// widows can merge runs back across several earlier breaks without undoing shifts. Phase 2 then
        /// applies the shifts in one pass.
        /// </remarks>
        internal static void ApplyLineBreaking(CssBox blockBox)
        {
            var container = blockBox.HtmlContainer;
            // A fixed box repeats on every page in page coordinates and isn't paginated (css-position-3).
            if (container == null || !container.HasRealPageGrid || blockBox.IsFixed)
                return;

            var lines = blockBox.LineBoxes;
            if (lines.Count == 0)
                return;

            var originalTop = lines[0].LineTop;

            var orphans = blockBox.ActualOrphans;
            var widows = blockBox.ActualWidows;
            var pageHeight = container.PageSize.Height;

            // The first run only has the room left on the page the box starts on, every later run starts
            // at the top of a page.
            var firstPageIndex = container.PageIndexOf(lines[0].LineTop);
            var firstRunCapacity = container.PageBottomOf(firstPageIndex) - lines[0].LineTop;

            var firstRunLineCount = 0;
            while (firstRunLineCount < lines.Count && lines[firstRunLineCount].LineBottom - lines[0].LineTop <= firstRunCapacity)
                firstRunLineCount++;

            // Fewer than `orphans` lines fit on the first page (or none at all), so the whole first run
            // starts on the next page. Not when it already starts at the top of a page, the next page has
            // no more room than this one.
            var alreadyAtFreshPageTop = Math.Abs(lines[0].LineTop - container.PageTopOf(firstPageIndex)) < 0.01;
            var firstRunMovedToFreshPage = !alreadyAtFreshPageTop && firstRunLineCount < lines.Count && firstRunLineCount < orphans;
            if (firstRunMovedToFreshPage)
            {
                firstPageIndex++;
                firstRunCapacity = pageHeight;
            }

            var breaks = new List<int> { 0 };

            for (var i = 1; i < lines.Count; i++)
            {
                var runStart = breaks[breaks.Count - 1];
                var capacity = runStart == 0 ? firstRunCapacity : pageHeight;
                if (lines[i].LineBottom - lines[runStart].LineTop <= capacity)
                    continue;

                var linesBefore = i - runStart;
                if (linesBefore > 0 && linesBefore < orphans && breaks.Count > 1)
                {
                    // Too few lines for orphans, merge this run into the previous one and test line i again.
                    breaks.RemoveAt(breaks.Count - 1);
                    i--;
                }
                else if (linesBefore > 0 && linesBefore < orphans && runStart == 0 && !firstRunMovedToFreshPage && !alreadyAtFreshPageTop)
                {
                    // Same for the first run, which has nothing to merge into, so it moves to the next page.
                    firstRunMovedToFreshPage = true;
                    firstPageIndex++;
                    firstRunCapacity = pageHeight;
                    i = 0; // Restarts at line 1 with the new capacity.
                }
                else
                {
                    breaks.Add(i);
                }
            }

            // Widows: move the last break earlier by as few lines as possible while the earlier run keeps
            // `orphans` lines. Failing that, merge the last run into the earlier one, and if that's the first
            // run and it doesn't fit its page, move the first run to the next page. If nothing fits, the
            // violation stays (CSS 2.1 §13.3.3 drops rule C).
            while (breaks.Count > 1 && lines.Count - breaks[breaks.Count - 1] < widows)
            {
                var prevRunStart = breaks[breaks.Count - 2];

                var shifted = false;
                for (var newBreak = breaks[breaks.Count - 1] - 1; newBreak > prevRunStart; newBreak--)
                {
                    if (newBreak - prevRunStart < orphans)
                        break;

                    if (lines.Count - newBreak < widows)
                        continue;

                    if (lines[lines.Count - 1].LineBottom - lines[newBreak].LineTop > pageHeight)
                        continue;

                    breaks[breaks.Count - 1] = newBreak;
                    shifted = true;
                    break;
                }

                if (shifted)
                    break;

                var mergedHeight = lines[lines.Count - 1].LineBottom - lines[prevRunStart].LineTop;
                if (mergedHeight <= (prevRunStart == 0 ? firstRunCapacity : pageHeight))
                {
                    breaks.RemoveAt(breaks.Count - 1);
                    continue;
                }

                if (prevRunStart == 0 && !firstRunMovedToFreshPage && mergedHeight <= pageHeight)
                {
                    firstRunMovedToFreshPage = true;
                    firstPageIndex++;
                    firstRunCapacity = pageHeight;
                    breaks.RemoveAt(breaks.Count - 1);
                    continue;
                }

                break;
            }

            // Phase 2
            var delta = firstRunMovedToFreshPage ? container.PageTopOf(firstPageIndex) - lines[0].LineTop : 0.0;
            var breakOrdinal = 0;

            for (var i = 0; i < lines.Count; i++)
            {
                if (breakOrdinal + 1 < breaks.Count && i == breaks[breakOrdinal + 1])
                {
                    breakOrdinal++;
                    var target = container.PageTopOf(firstPageIndex + breakOrdinal);
                    delta = target - lines[i].LineTop; // lines[i] isn't shifted yet.
                }

                if (delta != 0)
                    lines[i].ShiftLine(delta);
            }

            var maxBottom = 0.0;
            foreach (var line in lines)
            {
                maxBottom = Math.Max(maxBottom, line.LineBottom);
            }

            if (maxBottom > 0)
            {
                blockBox.ActualBottom = maxBottom + blockBox.ActualPaddingBottom + blockBox.ActualBorderBottomWidth;
            }

            BlockFragmentation.PropagateContainerRelocation(blockBox, lines[0].LineTop - originalTop);
        }
    }
}
