using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;

namespace TheArtOfDev.HtmlRenderer.Core.Paint.Content
{
    /// <summary>Paints an <c>&lt;iframe&gt;</c> fragment - the YouTube/Vimeo video thumbnail/title/play chrome.</summary>
    internal sealed class FrameFragmentPainter : ReplacedFragmentPainter
    {
        internal static readonly FrameFragmentPainter Instance = new FrameFragmentPainter();

        private FrameFragmentPainter()
        {
        }

        protected override void DrawContent(RGraphics g, BoxFragment fragment, RPoint offset)
        {
            var box = (CssBoxFrame)fragment.Box;
            box.EnsureVideoImageLoadStarted();
            box.DrawFrameContent(g, offset);
        }
    }
}
