using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Called right after <see cref="CssLayoutEngine.CreateLineBoxes"/> finishes for
        /// <paramref name="blockBox"/>: pushes any line that would land on a later page than its run's
        /// break down to that page's content top - and, honoring <c>orphans</c>/<c>widows</c>, the lines
        /// around it - then updates <see cref="CssBoxProperties.ActualBottom"/> to match.
        /// </summary>
        /// <remarks>
        /// Two phases, deliberately kept separate. Phase 1 decides every break index using each line's
        /// own NATURAL (never-shifted) position - a run's total height is preserved under a uniform
        /// shift, so "does a candidate run fit on one page" (and therefore where the next break falls)
        /// can be decided without knowing where the run will actually land. This is what lets widows
        /// cascade backward across more than one earlier break when needed (by removing entries from the
        /// decided break list) without having to undo a shift already applied to specific lines - an
        /// earlier single-pass version of this method shifted lines incrementally as it went, which
        /// couldn't cleanly support that. It also had a subtler failure mode worth recording: once a
        /// shift happens to land a run's lines in perfect page-boundary alignment (uniform line heights
        /// make this common), no line ever straddles again, so a single-pass method driven purely by "did
        /// this line straddle" silently stopped checking orphans/widows for every later page transition -
        /// found via a paragraph long enough to span dozens of pages, whose final page ended up with
        /// fewer lines than <c>widows</c> required and was never corrected. Phase 1's height-cumulative
        /// natural-position test has no such blind spot, since it never depends on whether a straddle was
        /// observed. Phase 2 applies the decided breaks as cumulative shifts to the real line boxes, in
        /// one forward pass - no decisions left to make there, just arithmetic.
        /// </remarks>
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
            var pageHeight = container.PageSize.Height;

            // The first run starts wherever CreateLineBoxes naturally placed line 0 - not necessarily a
            // page's top (this box may start partway down a page, after preceding sibling content) - so
            // its capacity is only whatever room remains on that page, not a full page height the way
            // every later run (which always starts fresh at a page's top, by construction) gets.
            var firstPageIndex = container.PageIndexOf(lines[0].LineTop);
            var firstRunCapacity = container.PageBottomOf(firstPageIndex) - lines[0].LineTop;

            // The box's own first line can itself fail to fit the room remaining on the page it starts
            // on (this box may start very close to a page's bottom) - every OTHER run always starts
            // fresh at a full page's top, where this can't happen unless a single line is individually
            // taller than a whole page (an unrelated, unhandled-here monolithic-overflow concern the
            // main loop's ordinary straddle test still catches the same way it always did). The main
            // loop below only ever compares a later line's cumulative height back to line 0's position -
            // it never re-examines whether line 0 itself already overflowed there, so this has to be
            // decided first and folded into where the first run is considered to begin.
            var firstLineNeedsOwnPage = lines[0].LineBottom - lines[0].LineTop > firstRunCapacity;
            if (firstLineNeedsOwnPage)
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
                    continue; // line i still fits in the run that started at runStart

                var linesBefore = i - runStart;
                if (linesBefore > 0 && linesBefore < orphans && breaks.Count > 1)
                {
                    // Too few lines to justify breaking here - the attempted run merges into the
                    // previous page's run instead of leaving a near-empty fragment behind. Re-test this
                    // same line against the now-earlier run start (cascades further back if needed).
                    breaks.RemoveAt(breaks.Count - 1);
                    i--;
                }
                else
                {
                    breaks.Add(i);
                }
            }

            // Widows: the run after the LAST break must have at least `widows` lines - if not, merge
            // break points backward (as many as needed) until it does, or until only one run is left, or
            // until merging further would make the run taller than a page can hold - honoring widows by
            // creating a run that can never fit isn't honoring it, it's trading one violation for a worse
            // one, so this is where the relaxation gives up rather than forcing it (css-break-3 §4.3's
            // own "some constraints can't always be satisfied" philosophy).
            while (breaks.Count > 1 && lines.Count - breaks[breaks.Count - 1] < widows)
            {
                var candidateStart = breaks[breaks.Count - 2];
                var candidateCapacity = candidateStart == 0 ? firstRunCapacity : pageHeight;
                if (lines[lines.Count - 1].LineBottom - lines[candidateStart].LineTop > candidateCapacity)
                    break;

                breaks.RemoveAt(breaks.Count - 1);
            }

            // Phase 2: apply the decided breaks as cumulative shifts, in one forward pass. The first
            // run's own delta is seeded up front (zero unless firstLineNeedsOwnPage moved it) since the
            // loop below only assigns a fresh delta when it crosses breaks[1] onward.
            var delta = firstLineNeedsOwnPage ? container.PageTopOf(firstPageIndex) - lines[0].LineTop : 0.0;
            var breakOrdinal = 0;

            for (var i = 0; i < lines.Count; i++)
            {
                if (breakOrdinal + 1 < breaks.Count && i == breaks[breakOrdinal + 1])
                {
                    breakOrdinal++;
                    var target = container.PageTopOf(firstPageIndex + breakOrdinal);
                    delta = target - lines[i].LineTop; // lines[i] not yet shifted this pass
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
        }
    }
}
