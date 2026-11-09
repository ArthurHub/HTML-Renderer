using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Paint.Content
{
    /// <summary>
    /// Paints a replaced element (<see cref="CssBoxImage"/>, <see cref="CssBoxFrame"/>): clip by overflow,
    /// background, border and outline, then the element's own content. Replaced elements are monolithic
    /// (CSS Fragmentation 3 §4.1), so <see cref="BoxFragment.PrimaryRect"/> covers the whole box.
    /// </summary>
    internal abstract class ReplacedFragmentPainter : IFragmentContentPainter
    {
        public void Paint(FragmentPainter painter, RGraphics g, BoxFragment fragment)
        {
            var box = fragment.Box;

            // PrimaryRect is fragment-local, the image word DrawContent reads is in document coordinates
            // of the live tree, so each gets its own offset.
            var rect = fragment.PrimaryRect;
            rect.Offset(painter.FragmentLocalOffset(box.IsFixed));

            var clipped = RenderUtils.ClipGraphicsByOverflow(g, box, painter.LiveTreeExtraOffset(box.IsFixed));

            box.PaintBackground(g, rect, true, true);
            BordersDrawHandler.DrawBoxBorders(g, box, rect, true, true);
            OutlineDrawHandler.Draw(g, box, rect);

            DrawContent(g, fragment, painter.LiveTreeOffset(box.IsFixed));

            if (clipped)
                g.PopClip();
        }

        protected abstract void DrawContent(RGraphics g, BoxFragment fragment, RPoint offset);
    }
}
