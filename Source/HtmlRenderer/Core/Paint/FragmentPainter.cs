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
    /// Paints a fragmentainer from the fragment tree. Geometry comes from the fragments, the
    /// <see cref="BoxFragment.Box"/> only provides the computed style and the paint primitives.
    /// Replaced and other leaf boxes are painted by their <see cref="Content.IFragmentContentPainter"/>.
    /// </summary>
    internal sealed class FragmentPainter
    {
        private readonly HtmlContainerInt _container;

        /// <summary>
        /// Added to every painted rect on top of the scroll offset. Only non-zero when several
        /// fragmentainers are painted onto one continuous surface, there it moves each band back to its
        /// document position.
        /// </summary>
        private readonly RPoint _pageOrigin;

        /// <summary>
        /// Document Y of the top of the fragmentainer being painted. Fragment geometry is already relative
        /// to it, geometry read from the live <see cref="CssBox"/> tree is not and has to subtract it.
        /// </summary>
        private double _bandTop;

        internal FragmentPainter(HtmlContainerInt container, RPoint pageOrigin = default)
        {
            _container = container;
            _pageOrigin = pageOrigin;
        }

        /// <summary>
        /// Offset from fragment geometry to paint position: scroll offset (not for fixed boxes) plus
        /// <see cref="_pageOrigin"/>.
        /// </summary>
        internal RPoint FragmentLocalOffset(bool isFixed)
        {
            var scroll = isFixed ? RPoint.Empty : _container.ScrollOffset;
            return new RPoint(scroll.X + _pageOrigin.X, scroll.Y + _pageOrigin.Y);
        }

        /// <summary>
        /// Offset from live box tree geometry to paint position. A fixed box is laid out relative to the
        /// page already, so the band top isn't subtracted for it.
        /// </summary>
        internal RPoint LiveTreeOffset(bool isFixed)
        {
            var offset = FragmentLocalOffset(isFixed);
            return isFixed ? offset : new RPoint(offset.X, offset.Y - _bandTop);
        }

        /// <summary>
        /// <see cref="LiveTreeOffset"/> without the scroll offset, which
        /// <see cref="RenderUtils.ClipGraphicsByOverflow"/> applies itself.
        /// </summary>
        internal RPoint LiveTreeExtraOffset(bool isFixed) =>
            isFixed ? new RPoint(_pageOrigin.X, _pageOrigin.Y) : new RPoint(_pageOrigin.X, _pageOrigin.Y - _bandTop);

        internal void Paint(RGraphics g, FragmentainerFragment fragmentainer)
        {
            _bandTop = fragmentainer.LocalOriginY;
            PaintFragment(g, fragmentainer.Root);
        }

        /// <summary>
        /// Paints one fragment and its descendants without the rest of its fragmentainer, for tests.
        /// <paramref name="bandTop"/> is the owning fragmentainer's <see cref="FragmentainerFragment.LocalOriginY"/>.
        /// </summary>
        internal void PaintFragmentSubtree(RGraphics g, BoxFragment fragment, double bandTop = 0)
        {
            _bandTop = bandTop;
            PaintFragment(g, fragment);
        }

        /// <summary>
        /// Paints one box fragment and its descendants, skipping it when it's hidden (CSS 2.1 §11.2) or
        /// lies entirely outside the current clip.
        /// </summary>
        private void PaintFragment(RGraphics g, BoxFragment fragment)
        {
            var box = fragment.Box;
            try
            {
                // Returning here also skips descendants that set 'visibility: visible' again.
                if (box.Display == CssConstants.None || box.Visibility != CssConstants.Visible)
                    return;

                // CSS 2.1 §11.1.1, the containing block of a fixed box is the viewport, so the clips of its
                // ancestors don't apply.
                var suspendsClip = box.Position == CssConstants.Fixed;
                if (suspendsClip)
                    g.SuspendClipping();

                var visible = box.Rectangles.Count == 0;
                if (!visible)
                {
                    // The containing block's rectangle is in document coordinates, unlike fragment geometry.
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
        /// Paints the box's background, border and outline per line, then its text and text decorations,
        /// then its children (CSS 2.1 Appendix E, within this box only).
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

            var clipped = RenderUtils.ClipGraphicsByOverflow(g, box, LiveTreeExtraOffset(box.IsFixed));
            var clip = g.GetClip();
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
                    OutlineDrawHandler.Draw(g, box, actualRect);
                }
            }

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

            // Normal flow, then absolute, then fixed, per parent rather than per stacking context (CSS 2.1 §9.9.1)
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

            // The marker is outside the principal box, so the box's overflow clip doesn't apply to it.
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
