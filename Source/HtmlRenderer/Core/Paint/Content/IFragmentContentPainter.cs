using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Core.Fragments;

namespace TheArtOfDev.HtmlRenderer.Core.Paint.Content
{
    /// <summary>
    /// Paints one box fragment's own replaced/leaf content - the per-type half of
    /// <see cref="FragmentPainter"/>'s dispatch (see <see cref="FragmentContentPainters.For"/>), matching
    /// PeachPDF's <c>IFragmentContentPainter</c> shape. Implementations are stateless singletons.
    /// </summary>
    internal interface IFragmentContentPainter
    {
        void Paint(FragmentPainter painter, RGraphics g, BoxFragment fragment);
    }
}
