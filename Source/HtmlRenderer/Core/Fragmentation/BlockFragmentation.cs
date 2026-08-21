using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Block-level page-break decisions. Being replaced, stage by stage, with real resumable-pass-loop
    /// equivalents matching PeachPDF's architecture (see the fragmentation-engine-parity plan) - forced
    /// breaks (<see cref="TryGetForcedBreakTarget"/>) already go through <c>CssBox</c>'s real pass loop
    /// as of that plan's R1. Margin truncation (<see cref="ResolveBlockTop"/>) and relocation
    /// (<see cref="RelocateIfNeeded"/>) remain local, single-pass corrections for now - they only need a
    /// box's own natural position, or its already-finished height, and never re-enter content that
    /// hasn't been measured yet - until later plan stages replace them too.
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
        /// subtree) has finished laying out. If the child straddles a page boundary and either asks not
        /// to be broken (<c>break-inside: avoid</c>) or may not be broken at all (a replaced element, a
        /// scroll container), and it fits within a single page's height, the child - and any preceding
        /// siblings chained to it by <c>break-after</c>/<c>break-before: avoid</c> (keep-with-next,
        /// css-break-3 §3.1) - are shifted down to the next page's content top.
        /// </summary>
        internal static void RelocateIfNeeded(CssBox child)
        {
            var container = child.HtmlContainer;
            if (container == null || !container.HasRealPageGrid || child.IsOutOfFlow)
                return;

            var top = child.Location.Y;
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
            var delta = target - top;

            foreach (var member in CollectPrecedingKeepWithNextRun(child))
            {
                member.OffsetTop(delta);
            }

            child.OffsetTop(delta);
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
