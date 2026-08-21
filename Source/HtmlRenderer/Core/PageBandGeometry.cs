namespace TheArtOfDev.HtmlRenderer.Core
{
    /// <summary>
    /// The resolved block-axis band and margins one fragmentainer (page) occupies, in true output units.
    /// HTML-Renderer keeps a single fixed page size/margins per document (no per-page @page overrides,
    /// unlike PeachPDF's variable-geometry PageGeometryTable), so this is a plain value computed once
    /// from the container's <see cref="HtmlContainerInt.PageSize"/> and margins rather than a table.
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
