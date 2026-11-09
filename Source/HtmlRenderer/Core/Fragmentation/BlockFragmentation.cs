using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Page break decisions between and around block-level boxes (CSS 2.1 §13.3, css-break-3 §3 to §5).
    /// </summary>
    internal static class BlockFragmentation
    {
        /// <summary>
        /// Resolves a block box's top, truncating a collapsed margin that crosses a page boundary at an
        /// unforced break (CSS 2.1 §13.3.3, css-break-3 §5.2). <paramref name="baseTopWithoutMargin"/> is
        /// the position before the box's own collapsed top margin, i.e. the containing block's content top
        /// or the previous sibling's bottom.
        /// </summary>
        internal static double ResolveBlockTop(CssBox box, CssBox prevSibling, double baseTopWithoutMargin)
        {
            var naturalTop = baseTopWithoutMargin + box.MarginTopCollapse(prevSibling);

            var container = box.HtmlContainer;
            if (container == null || !container.HasRealPageGrid)
                return naturalTop;

            var baseSlot = container.PageIndexOf(baseTopWithoutMargin);
            var naturalSlot = container.PageIndexOf(naturalTop);
            return naturalSlot > baseSlot ? container.PageTopOf(baseSlot + 1) : naturalTop;
        }

        /// <summary>
        /// Whether <paramref name="box"/> has a forced break before it (its own <c>break-before</c> or
        /// <paramref name="prevSibling"/>'s <c>break-after</c>, CSS 2.1 §13.3.4) that its natural top doesn't
        /// already satisfy, and if so the page slot and top it moves to.
        /// </summary>
        /// <remarks>
        /// Without a previous sibling the break belongs to an ancestor (css-break-3 §3.1). It isn't
        /// propagated, it's dropped, which also keeps a heading at the top of the document from opening a
        /// blank page.
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
                return false; // Already at the top of a page, breaking again would leave a blank page.

            slot = naturalSlot + 1;
            targetTop = container.PageTopOf(slot);
            return true;
        }

        /// <summary>
        /// Moves <paramref name="child"/> to the top of the next page when it straddles a page boundary but
        /// must not be broken, because of <c>break-inside: avoid</c> (CSS 2.1 §13.3.3 rule D) or because it's
        /// monolithic (css-break-3 §2). A child taller than a page stays where it is, CSS 2.1 §13.3.3 drops
        /// rule D when there aren't enough break points.
        /// </summary>
        /// <remarks>
        /// Called by the parent's child loop right after the child is laid out. Nothing after the child has
        /// been laid out yet, so the child is laid out again at its new top instead of being shifted, which
        /// lets its own descendants decide their breaks against the new position.
        /// </remarks>
        internal static void RelocateIfNeeded(RGraphics g, CssBox child)
        {
            var container = child.HtmlContainer;
            if (container == null || !container.HasRealPageGrid || child.IsOutOfFlow || child.IsFixed)
                return;

            var top = child.EffectiveTop;
            var bottom = child.ActualBottom;
            if (bottom <= top)
                return;

            var topSlot = container.PageIndexOf(top);
            // A bottom exactly on a boundary belongs to the page above it.
            var bottomSlot = container.PageIndexOf(Math.Max(top, bottom - 0.01));
            if (bottomSlot <= topSlot)
                return;

            if (!BreakValues.AvoidsBreak(child.BreakInside) && !MonolithicContent.IsMonolithic(child))
                return;

            var height = bottom - top;
            if (height > container.PageSize.Height)
                return;

            var target = container.PageTopOf(topSlot + 1);
            child.ResumeAt(null, target);
            child.PerformLayout(g);

            PropagateContainerRelocation(child, child.EffectiveTop - top);
        }

        /// <summary>
        /// When a page break falls between <paramref name="child"/> and its previous in-flow sibling and
        /// either of them avoids it (<c>break-after</c>/<c>break-before: avoid</c>, CSS 2.1 §13.3.3 rule A),
        /// pulls the preceding run of avoid-chained siblings onto <paramref name="child"/>'s page and lays
        /// <paramref name="child"/> out again below them.
        /// </summary>
        /// <remarks>
        /// The run is trimmed from its front until it fits on the page together with <paramref name="child"/>,
        /// if not even its last member fits nothing moves (css-break-3 §4.3). Pulling a run that can't fit
        /// makes its later members trigger the same pull again and again.
        /// </remarks>
        internal static void EnforceKeepWithNext(RGraphics g, CssBox child)
        {
            var container = child.HtmlContainer;
            if (container == null || !container.HasRealPageGrid || child.IsOutOfFlow || child.IsFixed)
                return;

            var prevSibling = DomUtils.GetPreviousSibling(child);
            if (prevSibling == null || prevSibling.IsOutOfFlow)
                return;

            if (!BreakValues.AvoidsBreak(prevSibling.BreakAfter) && !BreakValues.AvoidsBreak(child.BreakBefore))
                return;

            var prevBottomSlot = container.PageIndexOf(Math.Max(prevSibling.EffectiveTop, prevSibling.ActualBottom - 0.01));
            var childTopBeforeRelayout = child.EffectiveTop;
            var childBottomBeforeRelayout = child.ActualBottom;
            var childTopSlot = container.PageIndexOf(childTopBeforeRelayout);
            if (childTopSlot <= prevBottomSlot)
                return; // No break between them.

            var run = CollectPrecedingKeepWithNextRun(prevSibling);
            run.Add(prevSibling);

            var childHeight = childBottomBeforeRelayout - childTopBeforeRelayout;
            var pageHeight = container.PageSize.Height;
            var start = 0;
            while (start < run.Count)
            {
                var runHeight = run[run.Count - 1].ActualBottom - run[start].EffectiveTop;
                if (runHeight + childHeight <= pageHeight)
                    break;
                start++;
            }

            if (start >= run.Count)
                return;

            var originalGroupTop = run[start].EffectiveTop; // Before OffsetTop below moves it.
            var delta = container.PageTopOf(childTopSlot) - originalGroupTop;
            if (delta <= 0)
                return;

            for (var i = start; i < run.Count; i++)
            {
                run[i].OffsetTop(delta);
            }

            child.ResumeAt(null, null);
            child.PerformLayout(g);

            PropagateContainerRelocation(run[start], delta);
        }

        /// <summary>
        /// The already positioned in-flow siblings before <paramref name="box"/> that are chained to it by
        /// <c>break-after</c>/<c>break-before: avoid</c>, in document order.
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

        /// <summary>
        /// While <paramref name="movedBox"/> is its parent's first in-flow child, the break before it is
        /// also the break before the parent (css-break-3 §3.1), so the parent's top moves by the same
        /// <paramref name="delta"/>, and so on up the ancestors.
        /// </summary>
        /// <remarks>
        /// Only the parent's top moves, its bottom still follows from its last child. The ancestors are in
        /// the middle of their own layout, so they're neither shifted as a subtree (that would move
        /// <paramref name="movedBox"/> twice) nor laid out again. Their list markers are positioned later in
        /// that same layout call and pick up the new top.
        /// </remarks>
        internal static void PropagateContainerRelocation(CssBox movedBox, double delta)
        {
            if (delta == 0)
                return;

            var current = movedBox;
            var parent = current.ParentBox;
            while (parent != null && DomUtils.GetPreviousSibling(current) == null)
            {
                parent.Location = new RPoint(parent.Location.X, parent.Location.Y + delta);
                current = parent;
                parent = parent.ParentBox;
            }
        }
    }
}
