// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
//
// - Sun Tsu,
// "The Art of War"

using SkiaSharp;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp
{
    /// <summary>
    /// The configuration for <see cref="PdfGenerator"/>: page size, margins and document metadata.
    /// </summary>
    /// <remarks>
    /// Sizes are in points (1/72 inch) like everything in a PDF, a CSS pixel is 0.75pt.
    /// </remarks>
    public sealed class PdfGenerateConfig
    {
        /// <summary>
        /// The size of each page in points, <see cref="PdfPageSizes.A4"/> by default.
        /// </summary>
        public SKSize PageSize { get; set; } = PdfPageSizes.A4;

        /// <summary>
        /// The space between the top of the page and the content, in points.
        /// </summary>
        public float MarginTop { get; set; } = 36;

        /// <summary>
        /// The space between the bottom of the page and the content, in points.
        /// </summary>
        public float MarginBottom { get; set; } = 36;

        /// <summary>
        /// The space between the left of the page and the content, in points.
        /// </summary>
        public float MarginLeft { get; set; } = 36;

        /// <summary>
        /// The space between the right of the page and the content, in points.
        /// </summary>
        public float MarginRight { get; set; } = 36;

        /// <summary>
        /// The document's title, author etc., <see cref="SKDocumentPdfMetadata.Default"/> by default.
        /// An encoding quality or raster DPI of 0 uses the value of <see cref="SKDocumentPdfMetadata.Default"/>.
        /// </summary>
        public SKDocumentPdfMetadata Metadata { get; set; } = SKDocumentPdfMetadata.Default;

        /// <summary>
        /// Set all 4 margins to the given value, in points.
        /// </summary>
        public void SetMargins(float value)
        {
            MarginTop = MarginBottom = MarginLeft = MarginRight = value;
        }
    }

    /// <summary>
    /// Common paper sizes in points, portrait.
    /// </summary>
    public static class PdfPageSizes
    {
        public static readonly SKSize A3 = new SKSize(842, 1191);

        public static readonly SKSize A4 = new SKSize(595, 842);

        public static readonly SKSize A5 = new SKSize(420, 595);

        public static readonly SKSize Letter = new SKSize(612, 792);

        public static readonly SKSize Legal = new SKSize(612, 1008);

        /// <summary>
        /// The same size turned by 90 degrees.
        /// </summary>
        public static SKSize Landscape(SKSize size)
        {
            return new SKSize(size.Height, size.Width);
        }
    }
}
