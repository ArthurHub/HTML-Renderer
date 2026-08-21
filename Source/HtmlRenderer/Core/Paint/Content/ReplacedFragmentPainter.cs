using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Paint.Content
{
    /// <summary>
    /// Shared clip/background/border sequence for replaced leaf elements (<see cref="CssBoxImage"/>,
    /// <see cref="CssBoxFrame"/>) - both paint the same way (clip by overflow, then
    /// <see cref="CssBox.PaintBackground"/>, then <see cref="BordersDrawHandler.DrawBoxBorders"/>) before
    /// their own type-specific content, matching PeachPDF's <c>ReplacedFragmentPainter</c> base. Uses
    /// <see cref="BoxFragment.PrimaryRect"/> rather than <c>CssBox.Rectangles</c> directly since replaced
    /// elements are monolithic (one fragment always covers the whole box, css-break-3 4.1).
    /// </summary>
    internal abstract class ReplacedFragmentPainter : IFragmentContentPainter
    {
        public void Paint(FragmentPainter painter, RGraphics g, BoxFragment fragment)
        {
            var box = fragment.Box;
            var offset = box.IsFixed ? RPoint.Empty : painter.Container.ScrollOffset;
            var rect = fragment.PrimaryRect;
            rect.Offset(offset);

            var clipped = RenderUtils.ClipGraphicsByOverflow(g, box);

            box.PaintBackground(g, rect, true, true);
            BordersDrawHandler.DrawBoxBorders(g, box, rect, true, true);

            DrawContent(g, fragment, offset);

            if (clipped)
                g.PopClip();
        }

        protected abstract void DrawContent(RGraphics g, BoxFragment fragment, RPoint offset);
    }
}
