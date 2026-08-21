using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Block-level page-break corrections applied as part of HTML-Renderer's existing single-pass
    /// positioning, rather than via PeachPDF's break-token/pass-loop model. Every correction here is
    /// local: it only needs a box's own natural position, or (for relocation) its already-finished
    /// height - none of them need multi-pass resumption, because they never re-enter content that
    /// hasn't been measured yet. Real resumption (BreakToken/FragmentainerContext) is reserved for
    /// where it's actually needed: inline flow (can't restart word measurement/hyphenation from
    /// scratch) and table row continuation.
    /// </summary>
    internal static class BlockFragmentation
    {
        /// <summary>
        /// Resolves a block box's document-space top, applying forced page breaks
        /// (<c>break-before</c>/<c>break-after: page</c>, including the legacy <c>always</c> value) and
        /// css-break-3 §5.2 margin truncation at unforced breaks. <paramref name="baseTopWithoutMargin"/>
        /// is the position before this box's own collapsed top margin is added (the containing block's
        /// content top, or the previous sibling's border-box bottom).
        /// </summary>
        internal static double ResolveBlockTop(CssBox box, CssBox prevSibling, double baseTopWithoutMargin)
        {
            var naturalTop = baseTopWithoutMargin + box.MarginTopCollapse(prevSibling);

            var container = box.HtmlContainer;
            if (container == null || !container.HasRealPageGrid)
                return naturalTop;

            // Suppressed when there's no previous sibling: css-break-3 §3.1 propagation says the break
            // point before a container's first in-flow child IS the break point before the container
            // itself - so a forced break here would really belong to an ancestor (and ultimately, if
            // that ancestor also has no previous sibling, to the fragmentation root, where it's
            // inherently inert - there's no earlier page to break away from). Full cross-ancestor
            // propagation is out of scope for this port; suppressing at the box's own level is what
            // keeps a heading that merely happens to be first on the page from forcing a spurious
            // leading blank page - the common case this UA default (`h1 { page-break-before: always }`)
            // exists for is a heading that starts a new section partway through a document, not one.
            var forcedBefore = prevSibling != null && BreakValues.IsForcedBreak(box.BreakBefore);
            var forcedAfter = prevSibling != null && BreakValues.IsForcedBreak(prevSibling.BreakAfter);

            if (forcedBefore || forcedAfter)
            {
                var slot = container.PageIndexOf(naturalTop);
                var pageTop = container.PageTopOf(slot);
                // Already flush at a fresh page's top - a forced break here does not skip a page.
                return naturalTop > pageTop + 0.01 ? container.PageTopOf(slot + 1) : naturalTop;
            }

            // css-break-3 §5.2: a collapsed margin that, by itself, pushes content across one or more
            // page boundaries is truncated to zero - content starts flush at the next page instead of
            // paginating through blank vertical space.
            var baseSlot = container.PageIndexOf(baseTopWithoutMargin);
            var naturalSlot = container.PageIndexOf(naturalTop);
            return naturalSlot > baseSlot ? container.PageTopOf(baseSlot + 1) : naturalTop;
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
