using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;

namespace TheArtOfDev.HtmlRenderer.Core.Paint.Content
{
    /// <summary>Paints an <c>&lt;img&gt;</c> fragment - image, or error/loading placeholder.</summary>
    internal sealed class ImageFragmentPainter : ReplacedFragmentPainter
    {
        internal static readonly ImageFragmentPainter Instance = new ImageFragmentPainter();

        private ImageFragmentPainter()
        {
        }

        protected override void DrawContent(RGraphics g, BoxFragment fragment, RPoint offset)
        {
            var box = (CssBoxImage)fragment.Box;
            box.EnsureImageLoadStarted();
            box.DrawImageContent(g, offset);
        }
    }
}
