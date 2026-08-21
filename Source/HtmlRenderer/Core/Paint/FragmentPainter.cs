using System;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Paint
{
    /// <summary>
    /// Paints a fragmentainer from the immutable fragment tree - the sole paint path now that the old
    /// live-tree walk (formerly <c>CssBox.Paint</c>/<c>PaintImp</c>) has been deleted. Every geometric
    /// decision reads from the <see cref="BoxFragment"/> being painted, including text: each word paints
    /// at its own <see cref="TextFragment.Rect"/>, not <c>CssRect.Rectangle</c> read off the live box.
    /// The box back-reference (<see cref="BoxFragment.Box"/>) is consulted only for computed style and
    /// paint primitives themselves (<see cref="CssBox.PaintBackground"/>/<see cref="CssBox.PaintWord"/>/
    /// <see cref="CssBox.PaintDecoration"/> - widened from <c>protected</c>/<c>private</c> to <c>internal</c>
    /// rather than duplicated here, so this stays a faithful re-shaping of the existing, tested paint code
    /// rather than a parallel reimplementation).
    /// </summary>
    /// <remarks>
    /// Leaf/replaced types dispatch to their own <see cref="Content.IFragmentContentPainter"/> (matching
    /// PeachPDF's <c>IFragmentContentPainter</c>/<c>FragmentContentPainters</c> shape, see
    /// <see cref="Content.FragmentContentPainters.For"/>); everything else uses the generic box-fragment
    /// path below. Stacking-context paint order and <c>box-decoration-break</c> slicing are follow-on
    /// work once real fragmentation (multiple fragments per box) exists for them to matter.
    /// </remarks>
    internal sealed class FragmentPainter
    {
        private readonly HtmlContainerInt _container;

        /// <summary>
        /// Added to every painted rect on top of <see cref="HtmlContainerInt.ScrollOffset"/> - zero for
        /// every ordinary caller (one fragmentainer already in its own native coordinate system: a PDF
        /// page's own <c>XGraphics</c>, or the single always-page-local-zero fragmentainer WinForms/WPF's
        /// continuous document produces). Non-zero only when <see cref="HtmlContainerInt.PerformPaint(RGraphics)"/>
        /// paints several fragmentainers onto one continuous surface (its multi-fragmentainer branch) -
        /// there each fragmentainer's content is fragment-tree-local (translated so the band's own top is
        /// Y=0) and must be translated back by the band's real document-Y top to land in the right place
        /// on the shared surface, matching what painting the old, unfragmented box tree once did directly.
        /// </summary>
        private readonly RPoint _pageOrigin;

        /// <summary>
        /// The real document-Y top of the fragmentainer currently being painted (<see cref="FragmentainerFragment.LocalOriginY"/>),
        /// set once per <see cref="Paint"/> call. Geometry sourced from the fragment tree (<see cref="BoxFragment.Lines"/>/
        /// <see cref="BoxFragment.Words"/>/<see cref="BoxFragment.PrimaryRect"/>) is already local to this band
        /// (<see cref="Fragmentation.FragmentEmitter"/> subtracts it at build time) and needs no further
        /// adjustment for it. Geometry read straight off the live <see cref="CssBox"/> tree instead
        /// (<see cref="CssBoxImage.DrawImageContent"/>'s image-word rect, the visibility cull below) is
        /// still absolute document-Y and must have this subtracted to land in the same target frame -
        /// missing this distinction for text was a real bug (found while building the continuous-surface
        /// paint path this field supports, since fixed by moving word painting onto the fragment tree
        /// entirely rather than reconciling it): every page after the first silently painted zero text,
        /// since a fresh per-page surface's origin is this band's top, not the document's.
        /// </summary>
        private double _bandTop;

        internal FragmentPainter(HtmlContainerInt container, RPoint pageOrigin = default)
        {
            _container = container;
            _pageOrigin = pageOrigin;
        }

        /// <summary>
        /// The offset to apply to a box's fragment-local rect (already local to the fragmentainer being
        /// painted) to reach its paint position: scroll offset (suppressed for a fixed-position box,
        /// matching the old live-tree walk's behavior) plus <see cref="_pageOrigin"/> (applies regardless
        /// of <paramref name="isFixed"/> - the old, single continuous-surface paint path this replaced
        /// never gave "fixed" boxes special treatment with respect to which page's content they belonged
        /// to, only whether scroll offset applied to them).
        /// </summary>
        internal RPoint FragmentLocalOffset(bool isFixed)
        {
            var scroll = isFixed ? RPoint.Empty : _container.ScrollOffset;
            return new RPoint(scroll.X + _pageOrigin.X, scroll.Y + _pageOrigin.Y);
        }

        /// <summary>
        /// The offset to apply to a rect read straight off the live <see cref="CssBox"/> tree (still
        /// absolute document-Y, unlike fragment-tree geometry) to reach the same paint position
        /// <see cref="FragmentLocalOffset"/> gives fragment-local geometry: additionally undoes
        /// <see cref="_bandTop"/>, regardless of <paramref name="isFixed"/> (band membership is
        /// orthogonal to scroll-offset suppression).
        /// </summary>
        internal RPoint LiveTreeOffset(bool isFixed)
        {
            var offset = FragmentLocalOffset(isFixed);
            return new RPoint(offset.X, offset.Y - _bandTop);
        }

        /// <summary>
        /// The portion of <see cref="LiveTreeOffset"/> that <see cref="RenderUtils.ClipGraphicsByOverflow"/>
        /// doesn't already add itself (it applies <see cref="HtmlContainerInt.ScrollOffset"/>/<c>IsFixed</c>
        /// gating internally) - pass as its <c>extraOffset</c> parameter.
        /// </summary>
        internal RPoint LiveTreeExtraOffset => new RPoint(_pageOrigin.X, _pageOrigin.Y - _bandTop);

        internal void Paint(RGraphics g, FragmentainerFragment fragmentainer)
        {
            _bandTop = fragmentainer.LocalOriginY;
            PaintFragment(g, fragmentainer.Root);
        }

        /// <summary>
        /// Paints one box fragment: display/visibility gate, fixed-position clip suspension, and the
        /// same "is this rect actually in the visible area" cull the old live-tree walk used, before
        /// handing off to the box's own content.
        /// </summary>
        private void PaintFragment(RGraphics g, BoxFragment fragment)
        {
            var box = fragment.Box;
            try
            {
                if (box.Display == CssConstants.None || box.Visibility != CssConstants.Visible)
                    return;

                // Only this box's own Position, not IsFixed's ancestor-aware sense - matching the old live-tree walk.
                var suspendsClip = box.Position == CssConstants.Fixed;
                if (suspendsClip)
                    g.SuspendClipping();

                var visible = box.Rectangles.Count == 0;
                if (!visible)
                {
                    // box.ContainingBlock.ClientRectangle is read off the live box tree - still absolute
                    // document-Y, unlike fragment-tree geometry, so this needs LiveTreeOffset (not just
                    // ScrollOffset) to land in this painter's target frame.
                    var clip = g.GetClip();
                    var rect = box.ContainingBlock.ClientRectangle;
                    rect.X -= 2;
                    rect.Width += 2;
                    rect.Offset(LiveTreeOffset(box.IsFixed));
                    clip.Intersect(rect);
                    visible = clip != RRect.Empty;
                }

                if (visible)
                    PaintFragmentContent(g, fragment);

                if (suspendsClip)
                    g.ResumeClipping();
            }
            catch (Exception ex)
            {
                _container.ReportError(HtmlRenderErrorType.Paint, "Exception in fragment paint", ex);
            }
        }

        /// <summary>
        /// Paints one box fragment's own decorations, words, and children.
        /// </summary>
        private void PaintFragmentContent(RGraphics g, BoxFragment fragment)
        {
            var box = fragment.Box;

            var contentPainter = Content.FragmentContentPainters.For(box);
            if (contentPainter != null)
            {
                contentPainter.Paint(this, g, fragment);
                return;
            }

            if (box.Display == CssConstants.None ||
                (box.Display == CssConstants.TableCell && box.EmptyCells == CssConstants.Hide && box.IsSpaceOrEmpty))
            {
                return;
            }

            var clipped = RenderUtils.ClipGraphicsByOverflow(g, box, LiveTreeExtraOffset);
            var clip = g.GetClip();
            // fragment.Lines/fragment.Words are already fragment-local (FragmentEmitter subtracted the
            // band top at build time) - only FragmentLocalOffset (scroll + page-origin) applies to either.
            var offset = FragmentLocalOffset(box.IsFixed);
            var lines = fragment.Lines;

            for (var i = 0; i < lines.Count; i++)
            {
                var actualRect = lines[i].Rect;
                actualRect.Offset(offset);
                if (IsRectVisible(actualRect, clip))
                {
                    box.PaintBackground(g, actualRect, i == 0, i == lines.Count - 1);
                    BordersDrawHandler.DrawBoxBorders(g, box, actualRect, i == 0, i == lines.Count - 1);
                }
            }

            // Width.Length > 0 gate matches CssBox's own former PaintWords guard - preserved here since
            // it's the caller's job now that word painting reads the fragment tree, not the live box.
            if (box.Width.Length > 0)
            {
                foreach (var wordFragment in fragment.Words)
                {
                    var wordRect = wordFragment.Rect;
                    wordRect.Offset(offset);
                    box.PaintWord(g, wordFragment.Word, wordRect);
                }
            }

            for (var i = 0; i < lines.Count; i++)
            {
                var actualRect = lines[i].Rect;
                actualRect.Offset(offset);
                if (IsRectVisible(actualRect, clip))
                {
                    box.PaintDecoration(g, actualRect, i == 0, i == lines.Count - 1);
                }
            }

            // Split to match the old live-tree walk's z-order: normal flow, then absolute, then fixed.
            foreach (var child in fragment.Children)
            {
                if (child.Box.Position != CssConstants.Absolute && !child.Box.IsFixed)
                    PaintFragment(g, child);
            }
            foreach (var child in fragment.Children)
            {
                if (child.Box.Position == CssConstants.Absolute)
                    PaintFragment(g, child);
            }
            foreach (var child in fragment.Children)
            {
                if (child.Box.IsFixed)
                    PaintFragment(g, child);
            }

            if (clipped)
                g.PopClip();

            // Marker paints last, after this fragment's own overflow clip is popped - see
            // BoxFragment.MarkerFragment's doc comment for why it's kept separate from Children.
            if (fragment.MarkerFragment != null)
                PaintFragment(g, fragment.MarkerFragment);
        }

        private static bool IsRectVisible(RRect rect, RRect clip)
        {
            rect.X -= 2;
            rect.Width += 2;
            clip.Intersect(rect);
            return clip != RRect.Empty;
        }
    }
}
