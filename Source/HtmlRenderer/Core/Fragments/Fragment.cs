using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace TheArtOfDev.HtmlRenderer.Core.Fragments
{
    /// <summary>
    /// The immutable output of layout - a "box fragment" per CSS Fragmentation Module Level 3 §2
    /// (https://www.w3.org/TR/css-break-3/#fragment). Layout produces this tree exactly once, at the end
    /// of <see cref="HtmlContainerInt.PerformLayout"/>; paint consumes it and must not read geometry off
    /// the mutable <see cref="CssBox"/> tree.
    /// </summary>
    /// <remarks>
    /// A <see cref="Fragment"/> owns geometry only - style and paint-handler dispatch are reached through
    /// <see cref="BoxFragment.Box"/> (a live <see cref="CssBox"/> back-reference), so fragments stay cheap
    /// and style keeps one home. Coordinates are fragmentainer-local: local.Y = documentY -
    /// fragmentainer.LocalOriginY, X unchanged (a page's horizontal margin is applied by the painter's own
    /// translate, not by layout).
    /// </remarks>
    internal abstract record Fragment(RRect Rect);

    /// <summary>
    /// Tells a decoration rectangle whether each of its four physical edges is a real box edge or a
    /// fragmentation break, for CSS <c>box-decoration-break</c> (css-break-3 §6.2). Not a <see cref="Fragment"/>
    /// itself - it's carried by a <see cref="LineFragment"/>. <paramref name="UnbrokenStrip"/> is what a
    /// <c>slice</c> value resolves against; <paramref name="FragmentRect"/> is what <c>clone</c> resolves against.
    /// </summary>
    internal sealed record SliceGeometry(
        RRect UnbrokenStrip,
        RRect FragmentRect,
        bool HasLeftEdge,
        bool HasRightEdge,
        bool HasTopEdge = true,
        bool HasBottomEdge = true);

    /// <summary>
    /// One line box's decoration rectangle - or, for a block-level box with no lines of its own, one rect
    /// covering the whole border box, with <see cref="Line"/> null. The fragment-tree analog of a single
    /// entry in a box's per-line paint rectangles.
    /// </summary>
    internal sealed record LineFragment(RRect Rect, CssLineBox Line, SliceGeometry Slice) : Fragment(Rect);

    /// <summary>One laid-out word (or inline replaced run). Words are monolithic - one <see cref="CssRect"/> maps to exactly one <see cref="TextFragment"/>.</summary>
    internal sealed record TextFragment(RRect Rect, CssRect Word) : Fragment(Rect);

    /// <summary>
    /// The portion of one <see cref="CssBox"/> living in one fragmentainer. A box spanning a page boundary
    /// produces one <see cref="BoxFragment"/> per page. <see cref="Lines"/>/<see cref="Words"/>/<see cref="Children"/>
    /// mirror what the old live-tree paint walk painted, in the same order: own decoration rects, own words,
    /// then stacking-ordered child box fragments. <see cref="MarkerFragment"/> (a list item's marker, if any)
    /// is kept separate from <see cref="Children"/> rather than folded in, matching <c>CssBox.PaintImp</c>'s
    /// own paint order - the marker paints last, after this fragment's own overflow clip is popped, since a
    /// <c>list-style-position: outside</c> marker can legitimately hang outside the content box's clip.
    /// </summary>
    internal sealed record BoxFragment(
        RRect Rect,
        CssBox Box,
        int FragmentainerIndex,
        double OriginY,
        RRect WholeBoxRect,
        bool IsFixed,
        bool IsFirstFragment,
        bool IsLastFragment,
        bool IsMonolithic,
        IReadOnlyList<LineFragment> Lines,
        IReadOnlyList<TextFragment> Words,
        IReadOnlyList<BoxFragment> Children,
        BoxFragment MarkerFragment,
        RRect? OverflowClip) : Fragment(Rect)
    {
        /// <summary>The rect a replaced element paints its background/border over: the first line's rect, else this fragment's own rect.</summary>
        public RRect PrimaryRect => Lines.Count > 0 ? Lines[0].Rect : Rect;

        /// <summary>Reference-equality lookup of a word's fragment rect within this box fragment.</summary>
        public bool TryGetWordRect(CssRect word, out RRect rect)
        {
            foreach (var text in Words)
            {
                if (ReferenceEquals(text.Word, word))
                {
                    rect = text.Rect;
                    return true;
                }
            }

            rect = default;
            return false;
        }
    }

    /// <summary>
    /// One page - one materialized fragmentainer. <see cref="SlotIndex"/> is the pagination-slot index this
    /// occupies; slot indices are not contiguous across <see cref="FragmentTree.Fragmentainers"/>, since a
    /// content-empty slot is never materialized (CSS Paged Media 3 §3.2). <see cref="Root"/> is the document
    /// root's <see cref="BoxFragment"/> for this page.
    /// </summary>
    internal sealed record FragmentainerFragment(
        RRect Rect,
        int SlotIndex,
        PageBandGeometry Geometry,
        double LocalOriginY,
        BoxFragment Root) : Fragment(Rect);

    /// <summary>The complete immutable result of laying out one document - fragmentainers in page order.</summary>
    internal sealed record FragmentTree(IReadOnlyList<FragmentainerFragment> Fragmentainers);
}
