using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

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
            // A fixed box (css-position-3, paged media) is repeated identically on every page and its
            // own coordinates are page-relative, not absolute document-Y (see FragmentEmitter's
            // CollectFixedRoots) - unlike a float or an absolutely-positioned box, which stay in normal
            // document flow and must still paginate like anything else, the UA "must not paginate the
            // content of fixed-positioned boxes" (css-position-3), so this correction does not apply.
            if (container == null || !container.HasRealPageGrid || blockBox.Position == CssConstants.Fixed)
                return;

            var lines = blockBox.LineBoxes;
            if (lines.Count == 0)
                return;

            // Captured before any shifting, for BlockFragmentation.PropagateContainerRelocation at the
            // end - see that method's own remarks for why css-break-3 §3.1 propagation applies here too,
            // not only to BlockFragmentation's own relocations: a box whose orphans violation pushes its
            // whole first run to a fresh page (below) moves its own EffectiveTop exactly the way
            // RelocateIfNeeded's block-level relocation does, and a parent that starts with this box
            // needs its own top to follow just the same.
            var originalTop = lines[0].LineTop;

            var orphans = blockBox.ActualOrphans;
            var widows = blockBox.ActualWidows;
            var pageHeight = container.PageSize.Height;

            // The first run starts wherever CreateLineBoxes naturally placed line 0 - not necessarily a
            // page's top (this box may start partway down a page, after preceding sibling content) - so
            // its capacity is only whatever room remains on that page, not a full page height the way
            // every later run (which always starts fresh at a page's top, by construction) gets.
            var firstPageIndex = container.PageIndexOf(lines[0].LineTop);
            var firstRunCapacity = container.PageBottomOf(firstPageIndex) - lines[0].LineTop;

            // How many lines actually fit in the room remaining on the page this box starts on - a
            // run's total height measured from line 0 is invariant under a uniform shift (see the
            // two-phase remark above), so this natural-position count is valid regardless of where the
            // run ends up landing.
            var firstRunLineCount = 0;
            while (firstRunLineCount < lines.Count && lines[firstRunLineCount].LineBottom - lines[0].LineTop <= firstRunCapacity)
                firstRunLineCount++;

            // Orphans (css-break-3 §5.4) applies to the box's very first run exactly like every later
            // one: a paragraph starting close enough to a page's bottom that fewer than `orphans` lines
            // fit there must move in its ENTIRETY to the next page, not leave a too-small first fragment
            // behind. The main loop below cannot fix this on its own - its merge-back correction only
            // ever runs once at least one earlier break already exists (`breaks.Count > 1`), which is
            // never true while still deciding the first run, so an otherwise-identical violation at the
            // very start of a paragraph was silently exempt. Folding it into where the first run begins
            // (the same mechanism already used for a single first line taller than the remaining room)
            // fixes it without needing a special case in the main loop. Subsumes that single-line case
            // too - it is just the `orphans` violation that can never be waived (0 lines fitting is
            // always fewer than any orphans value of at least 1).
            // A forced break (or any other placement) may already have put this run flush at a fresh
            // page's own top - in which case its capacity IS a full page height already, and pushing it
            // to yet ANOTHER fresh page cannot gain any more room (same content, same capacity, same
            // unsatisfiable result), it would just leave the page it was actually placed on blank. Only
            // worth doing when there is real room being left on the table by staying put.
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
                else if (linesBefore > 0 && linesBefore < orphans && runStart == 0 && !firstRunMovedToFreshPage && !alreadyAtFreshPageTop)
                {
                    // Same violation as above, but there is no earlier run to merge into - runStart==0
                    // IS the first run. The only fix here is exactly what the pre-loop check above already
                    // does for the common case: push it whole to a fresh page - gated by the SAME
                    // alreadyAtFreshPageTop condition that check uses, for the same reason: if this run is
                    // already sitting at a fresh page's own top, it already got the full pageHeight
                    // capacity and still could not fit `orphans` lines (the pre-loop check's own
                    // firstRunLineCount<orphans, computed against that same capacity, agrees) - genuinely
                    // unsatisfiable, not something a SECOND fresh page could help with either.
                    firstRunMovedToFreshPage = true;
                    firstPageIndex++;
                    firstRunCapacity = pageHeight;
                    i = 0; // restart the scan from line 1 under the widened capacity
                }
                else
                {
                    breaks.Add(i);
                }
            }

            // Widows: the run after the LAST break must have at least `widows` lines - if not, first try
            // shifting the break point EARLIER by as few lines as it takes (css-break-3 §5.4 asks for the
            // minimum number of lines moved across the break, not for the whole preceding run to be
            // absorbed) - as long as the shrinking earlier run still keeps at least `orphans` lines of its
            // own. A shifted-but-still-separate run always lands at a fresh page's own top regardless of
            // where exactly the break falls (phase 2 places every run but the first at
            // PageTopOf(firstPageIndex + its own ordinal), which does not depend on how many lines it
            // holds) - so its own capacity is always a full page height, never the tighter capacity a
            // merge into run 0 might face.
            //
            // If no such shift satisfies widows without violating orphans on the earlier run, fall back to
            // merging the whole earlier run away entirely (as many times as it takes, cascading further
            // back across more than one earlier break when needed) - first against that run's own current
            // capacity (a full page height, unless it is run 0, still sitting at whatever tighter room its
            // natural position left it), and if that specific case (merging into run 0) does not fit
            // there, against a full page height instead (mirroring firstRunMovedToFreshPage above: run 0
            // pushed whole to a fresh page gains the same capacity boost a later run already has for free).
            //
            // Only when neither a shift nor a merge - at any capacity - can satisfy widows does this give
            // up and leave the violation in place: honoring widows by creating a run that can never fit
            // isn't honoring it, it's trading one violation for a worse one (css-break-3 §4.3's own "some
            // constraints can't always be satisfied" philosophy).
            while (breaks.Count > 1 && lines.Count - breaks[breaks.Count - 1] < widows)
            {
                var prevRunStart = breaks[breaks.Count - 2];

                var shifted = false;
                for (var newBreak = breaks[breaks.Count - 1] - 1; newBreak > prevRunStart; newBreak--)
                {
                    if (newBreak - prevRunStart < orphans)
                        break; // shifting further would strand the earlier run below its own orphans minimum

                    if (lines.Count - newBreak < widows)
                        continue; // this candidate still does not have enough lines after it

                    if (lines[lines.Count - 1].LineBottom - lines[newBreak].LineTop > pageHeight)
                        continue; // the (now larger) last run would not fit a fresh page either

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
                    continue; // re-test widows against the now one-level-earlier run (cascades further back)
                }

                if (prevRunStart == 0 && !firstRunMovedToFreshPage && mergedHeight <= pageHeight)
                {
                    // Merging everything back into run 0 does not fit run 0's own natural (tighter) room,
                    // but WOULD fit a full page - exactly the boost firstRunMovedToFreshPage already gives
                    // the first run for orphans. Apply it here too and finish the merge.
                    firstRunMovedToFreshPage = true;
                    firstPageIndex++;
                    firstRunCapacity = pageHeight;
                    breaks.RemoveAt(breaks.Count - 1);
                    continue;
                }

                break; // neither a shift nor any merge can satisfy widows - decline gracefully
            }

            // Phase 2: apply the decided breaks as cumulative shifts, in one forward pass. The first
            // run's own delta is seeded up front (zero unless firstRunMovedToFreshPage moved it) since
            // the loop below only assigns a fresh delta when it crosses breaks[1] onward.
            var delta = firstRunMovedToFreshPage ? container.PageTopOf(firstPageIndex) - lines[0].LineTop : 0.0;
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

            BlockFragmentation.PropagateContainerRelocation(blockBox, lines[0].LineTop - originalTop);
        }
    }
}
