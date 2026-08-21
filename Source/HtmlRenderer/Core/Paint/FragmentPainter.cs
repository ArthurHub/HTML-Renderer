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
    /// Paints a fragmentainer from the immutable fragment tree, replacing <see cref="CssBox.Paint"/>'s
    /// live-tree walk. Every geometric decision reads from the <see cref="BoxFragment"/> being painted;
    /// the box back-reference (<see cref="BoxFragment.Box"/>) is consulted only for computed style and,
    /// for now, for the paint primitives themselves (<see cref="CssBox.PaintBackground"/>/
    /// <see cref="CssBox.PaintWords"/>/<see cref="CssBox.PaintDecoration"/> - widened from <c>protected</c>/
    /// <c>private</c> to <c>internal</c> rather than duplicated here, so this stays a faithful re-shaping of
    /// the existing, tested paint code rather than a parallel reimplementation).
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

        internal FragmentPainter(HtmlContainerInt container)
        {
            _container = container;
        }

        /// <summary>Exposed for <see cref="Content.IFragmentContentPainter"/> implementations, which live outside this class but need <see cref="HtmlContainerInt.ScrollOffset"/>.</summary>
        internal HtmlContainerInt Container => _container;

        internal void Paint(RGraphics g, FragmentainerFragment fragmentainer)
        {
            PaintFragment(g, fragmentainer.Root);
        }

        /// <summary>
        /// Paints one box fragment - the fragment-tree analog of <see cref="CssBox.Paint"/>: display/
        /// visibility gate, fixed-position clip suspension, and the same "is this rect actually in the
        /// visible area" cull, before handing off to the box's own content.
        /// </summary>
        private void PaintFragment(RGraphics g, BoxFragment fragment)
        {
            var box = fragment.Box;
            try
            {
                if (box.Display == CssConstants.None || box.Visibility != CssConstants.Visible)
                    return;

                // Only this box's own Position, not IsFixed's ancestor-aware sense - matching CssBox.Paint.
                var suspendsClip = box.Position == CssConstants.Fixed;
                if (suspendsClip)
                    g.SuspendClipping();

                var visible = box.Rectangles.Count == 0;
                if (!visible)
                {
                    var clip = g.GetClip();
                    var rect = box.ContainingBlock.ClientRectangle;
                    rect.X -= 2;
                    rect.Width += 2;
                    if (!box.IsFixed)
                        rect.Offset(_container.ScrollOffset);
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
        /// Paints one box fragment's own decorations, words, and children - the fragment-tree analog of
        /// <see cref="CssBox.PaintImp"/>.
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

            var clipped = RenderUtils.ClipGraphicsByOverflow(g, box);
            var clip = g.GetClip();
            var offset = box.IsFixed ? RPoint.Empty : _container.ScrollOffset;
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

            box.PaintWords(g, offset);

            for (var i = 0; i < lines.Count; i++)
            {
                var actualRect = lines[i].Rect;
                actualRect.Offset(offset);
                if (IsRectVisible(actualRect, clip))
                {
                    box.PaintDecoration(g, actualRect, i == 0, i == lines.Count - 1);
                }
            }

            // Split to match the z-order CssBox.PaintImp already uses: normal flow, then absolute, then fixed.
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
