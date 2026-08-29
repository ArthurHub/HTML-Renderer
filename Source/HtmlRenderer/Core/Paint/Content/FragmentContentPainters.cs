using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace TheArtOfDev.HtmlRenderer.Core.Paint.Content
{
    /// <summary>
    /// Dispatches a box to its <see cref="IFragmentContentPainter"/>, matching PeachPDF's
    /// <c>FragmentContentPainters.For</c>. A null result tells <see cref="FragmentPainter"/> the generic
    /// box-fragment path (background/border per line, words, decoration, stacking-ordered children)
    /// applies instead - every leaf/replaced type with its own paint shape is listed here explicitly.
    /// </summary>
    internal static class FragmentContentPainters
    {
        internal static IFragmentContentPainter For(CssBox box)
        {
            switch (box)
            {
                case CssBoxImage:
                    return ImageFragmentPainter.Instance;
                case CssBoxHr:
                    return HrFragmentPainter.Instance;
                case CssBoxFrame:
                    return FrameFragmentPainter.Instance;
                default:
                    return null;
            }
        }
    }
}
