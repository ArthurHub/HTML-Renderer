using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;

namespace TheArtOfDev.HtmlRenderer.Core.Paint.Content
{
    /// <summary>
    /// Paints an <c>&lt;hr&gt;</c> fragment. Not a <see cref="ReplacedFragmentPainter"/> - a rule draws
    /// each border edge itself rather than going through the shared background+<c>DrawBoxBorders</c> step
    /// (see <see cref="CssBoxHr.DrawHrContent"/>), matching PeachPDF's <c>HrFragmentPainter</c>.
    /// </summary>
    internal sealed class HrFragmentPainter : IFragmentContentPainter
    {
        internal static readonly HrFragmentPainter Instance = new HrFragmentPainter();

        private HrFragmentPainter()
        {
        }

        public void Paint(FragmentPainter painter, RGraphics g, BoxFragment fragment)
        {
            var box = (CssBoxHr)fragment.Box;
            var offset = painter.FragmentLocalOffset(box.IsFixed);
            var rect = fragment.PrimaryRect;
            rect.Offset(offset);
            box.DrawHrContent(g, rect);
        }
    }
}
