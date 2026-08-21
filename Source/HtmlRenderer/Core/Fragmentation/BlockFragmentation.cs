using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Block-level page-break decisions. Being replaced, stage by stage, with real resumable-pass-loop
    /// equivalents matching PeachPDF's architecture (see the fragmentation-engine-parity plan): forced
    /// breaks (<see cref="TryGetForcedBreakTarget"/>, plan R1) go through <c>CssBox</c>'s real pass loop
    /// across fragmentainers; <c>break-inside:avoid</c>/monolithic relocation (<see cref="RelocateIfNeeded"/>,
    /// plan R3) and keep-with-next (<see cref="EnforceKeepWithNext"/>, plan R4) both relay the affected
    /// box out fresh at its target position within the SAME pass, rather than shifting already-finished
    /// geometry - real relayout, but not yet a cross-pass token, since nothing downstream has been touched
    /// yet when either fires. Margin truncation (<see cref="ResolveBlockTop"/>) remains the older
    /// pre-placement arithmetic correction, since it needs no relayout at all - it's already applied
    /// before a box is ever positioned, the same timing <see cref="TryGetForcedBreakTarget"/> uses.
    /// </summary>
    internal static class BlockFragmentation
    {
        /// <summary>
        /// Resolves a block box's document-space top, applying css-break-3 §5.2 margin truncation at
        /// unforced breaks. Forced <c>break-before</c>/<c>break-after: page</c> is handled earlier, by
        /// <see cref="TryGetForcedBreakTarget"/> and <c>CssBox</c>'s own pass loop - a box this method is
        /// reached for has already been confirmed not to have a forced break pending.
        /// <paramref name="baseTopWithoutMargin"/> is the position before this box's own collapsed top
        /// margin is added (the containing block's content top, or the previous sibling's border-box
        /// bottom).
        /// </summary>
        internal static double ResolveBlockTop(CssBox box, CssBox prevSibling, double baseTopWithoutMargin)
        {
            var naturalTop = baseTopWithoutMargin + box.MarginTopCollapse(prevSibling);

            var container = box.HtmlContainer;
            if (container == null || !container.HasRealPageGrid)
                return naturalTop;

            // css-break-3 §5.2: a collapsed margin that, by itself, pushes content across one or more
            // page boundaries is truncated to zero - content starts flush at the next page instead of
            // paginating through blank vertical space.
            var baseSlot = container.PageIndexOf(baseTopWithoutMargin);
            var naturalSlot = container.PageIndexOf(naturalTop);
            return naturalSlot > baseSlot ? container.PageTopOf(baseSlot + 1) : naturalTop;
        }

        /// <summary>
        /// Whether <paramref name="box"/> has a forced page break before it (its own <c>break-before</c>,
        /// or <paramref name="prevSibling"/>'s <c>break-after</c> - including the legacy <c>always</c>
        /// value) that isn't already satisfied by its natural top landing flush at a page top - and if so,
        /// the pagination slot/document-Y it must be deferred to. A box with a forced break pending is not
        /// placed this pass at all (see <c>CssBox.RequestedBreakBeforeTop</c>); its parent's child loop
        /// stops and the pass ends, resuming with this box placed fresh at <paramref name="targetTop"/>.
        /// </summary>
        /// <remarks>
        /// Suppressed when there's no previous sibling: css-break-3 §3.1 propagation says the break point
        /// before a container's first in-flow child IS the break point before the container itself - so a
        /// forced break here would really belong to an ancestor (and ultimately, if that ancestor also has
        /// no previous sibling, to the fragmentation root, where it's inherently inert - there's no earlier
        /// page to break away from). Full cross-ancestor propagation is out of scope for this port;
        /// suppressing at the box's own level is what keeps a heading that merely happens to be first on
        /// the page from forcing a spurious leading blank page - the common case this UA default
        /// (`h1 { page-break-before: always }`) exists for is a heading that starts a new section partway
        /// through a document, not one.
        /// </remarks>
        internal static bool TryGetForcedBreakTarget(CssBox box, CssBox prevSibling, double baseTopWithoutMargin, out int slot, out double targetTop)
        {
            slot = 0;
            targetTop = 0;

            var container = box.HtmlContainer;
            if (container == null || !container.HasRealPageGrid || prevSibling == null)
                return false;

            if (!BreakValues.IsForcedBreak(box.BreakBefore) && !BreakValues.IsForcedBreak(prevSibling.BreakAfter))
                return false;

            var naturalTop = baseTopWithoutMargin + box.MarginTopCollapse(prevSibling);
            var naturalSlot = container.PageIndexOf(naturalTop);
            var pageTop = container.PageTopOf(naturalSlot);
            if (naturalTop <= pageTop + 0.01)
                return false; // Already flush at a fresh page's top - a forced break here does not skip a page.

            slot = naturalSlot + 1;
            targetTop = container.PageTopOf(slot);
            return true;
        }

        /// <summary>
        /// Called by a block container's child loop right after <paramref name="child"/> (and its whole
        /// subtree) has finished laying out this pass. If the child straddles a page boundary and either
        /// asks not to be broken (<c>break-inside: avoid</c>) or may not be broken at all (a replaced
        /// element, a scroll container), and it fits within a single page's height, the child is relaid
        /// out fresh at the next page's content top. Does not itself consider whether this leaves a
        /// preceding sibling stranded - <see cref="EnforceKeepWithNext"/>, called right after this in the
        /// same loop iteration, catches that uniformly for every trigger (this one included).
        /// </summary>
        /// <remarks>
        /// The child is genuinely relaid out (<c>ResumeAt</c> + <c>PerformLayout</c>), not
        /// <c>OffsetTop</c>-shifted: nothing after this child in its parent's loop has been touched yet
        /// this pass, so re-entering its own layout at the new top is cheap, and it is also more correct
        /// than a flat shift - any of the child's OWN descendants that themselves have
        /// <c>break-inside:avoid</c> or a nested forced break get to make their own decision relative to
        /// the real page boundaries at the new position, rather than blindly carrying whatever decision
        /// they made at the old one.
        /// </remarks>
        internal static void RelocateIfNeeded(RGraphics g, CssBox child)
        {
            var container = child.HtmlContainer;
            if (container == null || !container.HasRealPageGrid || child.IsOutOfFlow)
                return;

            var top = child.EffectiveTop;
            var bottom = child.ActualBottom;
            if (bottom <= top)
                return;

            var topSlot = container.PageIndexOf(top);
            // Bottom-edge convention: a bottom landing exactly on a boundary belongs to the band above it.
            var bottomSlot = container.PageIndexOf(Math.Max(top, bottom - 0.01));
            if (bottomSlot <= topSlot)
                return;

            if (!BreakValues.AvoidsBreak(child.BreakInside) && !MonolithicContent.IsMonolithic(child))
                return;

            var height = bottom - top;
            if (height >= container.PageSize.Height)
                return; // Fits on no single page - left in place rather than moved somewhere it also won't fit.

            var target = container.PageTopOf(topSlot + 1);
            child.ResumeAt(null, target);
            child.PerformLayout(g);
        }

        /// <summary>
        /// Called by a block container's child loop right after <paramref name="child"/> has finished
        /// laying out (and, if applicable, been relocated by <see cref="RelocateIfNeeded"/>) this pass. If
        /// a page break actually falls between <paramref name="child"/> and its immediately preceding
        /// in-flow sibling, and either of them asks it not to (<c>break-after</c>/<c>break-before: avoid</c>,
        /// keep-with-next, css-break-3 §3.1), the whole preceding run chained to that sibling is pulled
        /// down to join <paramref name="child"/>'s page instead of leaving it stranded on the page it just
        /// left - then <paramref name="child"/> itself is relaid out fresh, since its own natural top
        /// depends on the now-shifted sibling's new bottom.
        /// </summary>
        /// <remarks>
        /// A real gap found while building this: the pre-existing keep-with-next code only ever ran as a side effect
        /// of <see cref="RelocateIfNeeded"/> relocating <paramref name="child"/> itself - so it only ever
        /// fired when <paramref name="child"/> was ALSO <c>break-inside:avoid</c> or monolithic. The
        /// ordinary case (an unremarkable paragraph that simply doesn't fit after a keep-with-next-chained
        /// heading) never triggered it at all: the heading was left stranded on the page it started on
        /// while the paragraph moved on alone. This method is the general fix - checked unconditionally,
        /// not only after a relocation - and <see cref="RelocateIfNeeded"/>'s own preceding-run handling
        /// was removed as redundant once this covers it too (after a relocation moves the child, the
        /// preceding sibling is exactly as "left behind" as in the ordinary case, and this method treats
        /// both identically).
        /// </remarks>
        internal static void EnforceKeepWithNext(RGraphics g, CssBox child)
        {
            var container = child.HtmlContainer;
            if (container == null || !container.HasRealPageGrid || child.IsOutOfFlow)
                return;

            var prevSibling = DomUtils.GetPreviousSibling(child);
            if (prevSibling == null || prevSibling.IsOutOfFlow)
                return;

            if (!BreakValues.AvoidsBreak(prevSibling.BreakAfter) && !BreakValues.AvoidsBreak(child.BreakBefore))
                return;

            var prevBottomSlot = container.PageIndexOf(Math.Max(prevSibling.EffectiveTop, prevSibling.ActualBottom - 0.01));
            var childTopSlot = container.PageIndexOf(child.EffectiveTop);
            if (childTopSlot <= prevBottomSlot)
                return; // No break actually falls between them - nothing to enforce.

            var run = CollectPrecedingKeepWithNextRun(prevSibling);
            run.Add(prevSibling);

            // Simplified for this stage: always pull the whole run to child's page, without checking
            // whether the run then fits alongside child there - the progressive relaxation ladder
            // (trim the run, drop it, leave the container behind) is a later plan stage's refinement.
            var delta = container.PageTopOf(childTopSlot) - run[0].EffectiveTop;
            if (delta <= 0)
                return; // Defensive - a positive shift is the only sensible outcome here.

            foreach (var member in run)
            {
                member.OffsetTop(delta);
            }

            child.ResumeAt(null, null);
            child.PerformLayout(g);
        }

        /// <summary>
        /// Walks backward through already-positioned preceding in-flow siblings chained to
        /// <paramref name="box"/> by <c>break-after</c>/<c>break-before: avoid</c> (css-break-3 §3.1),
        /// so a heading is never left stranded on the page its content just moved off of.
        /// </summary>
        private static List<CssBox> CollectPrecedingKeepWithNextRun(CssBox box)
        {
            var run = new List<CssBox>();
            var next = box;
            var current = DomUtils.GetPreviousSibling(box);

            while (current != null &&
                   (BreakValues.AvoidsBreak(current.BreakAfter) || BreakValues.AvoidsBreak(next.BreakBefore)))
            {
                run.Insert(0, current);
                next = current;
                current = DomUtils.GetPreviousSibling(current);
            }

            return run;
        }
    }
}
