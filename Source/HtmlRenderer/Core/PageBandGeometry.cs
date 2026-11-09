namespace TheArtOfDev.HtmlRenderer.Core
{
    /// <summary>
    /// The block-axis band and margins of one page, in output units. All pages share the container's
    /// <see cref="HtmlContainerInt.PageSize"/> and margins, <c>@page</c> rules aren't applied
    /// (CSS 2.1 §13.2).
    /// </summary>
    internal readonly struct PageBandGeometry
    {
        public PageBandGeometry(double top, double height, double marginTop, double marginRight, double marginBottom, double marginLeft)
        {
            Top = top;
            Height = height;
            MarginTop = marginTop;
            MarginRight = marginRight;
            MarginBottom = marginBottom;
            MarginLeft = marginLeft;
        }

        /// <summary>Document-space Y of the top of this fragmentainer's content band.</summary>
        public double Top { get; }

        /// <summary>The content band's block-axis extent.</summary>
        public double Height { get; }

        public double MarginTop { get; }
        public double MarginRight { get; }
        public double MarginBottom { get; }
        public double MarginLeft { get; }

        /// <summary>Document-space Y of the bottom of this fragmentainer's content band.</summary>
        public double Bottom => Top + Height;
    }
}
