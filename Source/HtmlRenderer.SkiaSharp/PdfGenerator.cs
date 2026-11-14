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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp
{
    /// <summary>
    /// Creates PDF documents from HTML with SkiaSharp's PDF backend, one page per page of the paginated layout.
    /// </summary>
    /// <remarks>
    /// The document is laid out for <c>@media print</c>, links to URLs and to elements of the document stay
    /// clickable. SkiaSharp's PDF backend embeds the complete font files, it doesn't subset them.
    /// </remarks>
    /// <example>
    /// using var stream = File.Create("document.pdf");<br/>
    /// await PdfGenerator.GeneratePdfAsync("<![CDATA[<h1>Hello World</h1>]]>", stream);<br/>
    /// </example>
    public static class PdfGenerator
    {
        /// <summary>
        /// CSS pixels to points, CSS 2.1 §4.3.2 fixes 1px at 1/96 inch.
        /// </summary>
        private const float PointsPerPixel = 72f / 96f;

        /// <summary>
        /// Create a PDF document from the given HTML and write it to the stream.
        /// </summary>
        /// <param name="html">HTML source to create the PDF from</param>
        /// <param name="stream">the stream to write the PDF to, left open</param>
        /// <param name="config">optional: page size, margins and metadata (default - A4 with 0.5 inch margins)</param>
        /// <param name="cssData">optional: the style to use for HTML rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the number of pages written</returns>
        public static async Task<int> GeneratePdfAsync(string html, Stream stream, PdfGenerateConfig config = null, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(stream, "stream");
            config = config ?? new PdfGenerateConfig();

            var pageWidth = config.PageSize.Width / PointsPerPixel;
            var pageHeight = config.PageSize.Height / PointsPerPixel;
            var marginLeft = (int)Math.Round(config.MarginLeft / PointsPerPixel);
            var marginTop = (int)Math.Round(config.MarginTop / PointsPerPixel);
            var marginRight = (int)Math.Round(config.MarginRight / PointsPerPixel);
            var marginBottom = (int)Math.Round(config.MarginBottom / PointsPerPixel);
            var contentSize = new RSize(pageWidth - marginLeft - marginRight, pageHeight - marginTop - marginBottom);

            using (var document = SKDocument.CreatePdf(stream, GetMetadata(config)))
            {
                var pageCount = 0;
                if (!string.IsNullOrEmpty(html))
                {
                    using (var container = new HtmlContainer())
                    {
                        if (stylesheetLoad != null)
                            container.StylesheetLoad += stylesheetLoad;
                        if (imageLoad != null)
                            container.ImageLoad += imageLoad;

                        container.MediaType = "print";
                        container.Location = new SKPoint(marginLeft, marginTop);
                        container.MaxSize = new SKSize((float)contentSize.Width, 0);
                        await container.SetHtml(html, cssData).ConfigureAwait(false);

                        var containerInt = container.HtmlContainerInt;
                        containerInt.PageSize = contentSize;
                        containerInt.MarginLeft = marginLeft;
                        containerInt.MarginTop = marginTop;
                        containerInt.MarginRight = marginRight;
                        containerInt.MarginBottom = marginBottom;
                        container.PerformLayout();

                        var fragmentainers = container.FragmentTree?.Fragmentainers ?? Array.Empty<FragmentainerFragment>();
                        var anchors = GetAnchorDestinations(container, fragmentainers);

                        foreach (var fragmentainer in fragmentainers)
                        {
                            var canvas = document.BeginPage(config.PageSize.Width, config.PageSize.Height);
                            canvas.Scale(PointsPerPixel);
                            canvas.ClipRect(SKRect.Create((float)pageWidth, (float)pageHeight));

                            container.PerformPaint(canvas, fragmentainer);
                            DrawLinks(canvas, container, fragmentainer, anchors);

                            document.EndPage();
                            pageCount++;
                        }
                    }
                }

                // A PDF needs a page, an empty document gets a blank one
                if (pageCount == 0)
                {
                    document.BeginPage(config.PageSize.Width, config.PageSize.Height);
                    document.EndPage();
                    pageCount = 1;
                }

                document.Close();
                return pageCount;
            }
        }

        /// <summary>
        /// Create a PDF document from the given HTML.
        /// </summary>
        /// <param name="html">HTML source to create the PDF from</param>
        /// <param name="config">optional: page size, margins and metadata (default - A4 with 0.5 inch margins)</param>
        /// <param name="cssData">optional: the style to use for HTML rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the PDF file's bytes</returns>
        public static async Task<byte[]> GeneratePdfAsync(string html, PdfGenerateConfig config = null, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            using (var stream = new MemoryStream())
            {
                await GeneratePdfAsync(html, stream, config, cssData, stylesheetLoad, imageLoad).ConfigureAwait(false);
                return stream.ToArray();
            }
        }

        #region Private methods

        /// <summary>
        /// <see cref="SKDocumentPdfMetadata"/> is a struct, so <c>new SKDocumentPdfMetadata { Title = ... }</c> leaves the encoding quality
        /// and raster DPI at 0 instead of the defaults, and Skia then stores every opaque image as a quality 0 JPEG.
        /// </summary>
        private static SKDocumentPdfMetadata GetMetadata(PdfGenerateConfig config)
        {
            var metadata = config.Metadata;
            if (metadata.EncodingQuality <= 0)
                metadata.EncodingQuality = SKDocumentPdfMetadata.Default.EncodingQuality;
            if (metadata.RasterDpi <= 0)
                metadata.RasterDpi = SKDocumentPdfMetadata.Default.RasterDpi;
            return metadata;
        }

        /// <summary>
        /// The page and position of each element an anchor link points to, keyed by the element id.
        /// </summary>
        private static Dictionary<string, (FragmentainerFragment Page, SKPoint Point)> GetAnchorDestinations(HtmlContainer container, IReadOnlyList<FragmentainerFragment> fragmentainers)
        {
            var destinations = new Dictionary<string, (FragmentainerFragment, SKPoint)>();
            foreach (var link in container.GetLinks())
            {
                if (!link.IsAnchor || link.AnchorId.Length == 0 || destinations.ContainsKey(link.AnchorId))
                    continue;

                var rect = container.GetElementRectangle(link.AnchorId);
                if (!rect.HasValue)
                    continue;

                var page = PageContaining(fragmentainers, rect.Value.Top);
                if (page != null)
                    destinations[link.AnchorId] = (page, new SKPoint(rect.Value.Left, (float)(rect.Value.Top - page.LocalOriginY)));
            }
            return destinations;
        }

        /// <summary>
        /// Adds the link annotations and anchor destinations that fall on the page.
        /// </summary>
        private static void DrawLinks(SKCanvas canvas, HtmlContainer container, FragmentainerFragment page, Dictionary<string, (FragmentainerFragment Page, SKPoint Point)> anchors)
        {
            foreach (var anchor in anchors)
            {
                if (anchor.Value.Page == page)
                    canvas.DrawNamedDestinationAnnotation(anchor.Value.Point, anchor.Key);
            }

            var bandTop = page.Geometry.Top;
            var bandBottom = bandTop + page.Geometry.Height;
            foreach (var link in container.GetLinks())
            {
                var rect = link.Rectangle;
                if (rect.Top >= bandBottom || rect.Bottom <= bandTop)
                    continue;

                var pageRect = SKRect.Create(rect.Left, (float)(rect.Top - page.LocalOriginY), rect.Width, rect.Height);
                if (link.IsAnchor)
                {
                    if (anchors.ContainsKey(link.AnchorId))
                        canvas.DrawLinkDestinationAnnotation(pageRect, link.AnchorId);
                }
                else if (!string.IsNullOrEmpty(link.Href))
                {
                    canvas.DrawUrlAnnotation(pageRect, link.Href);
                }
            }
        }

        private static FragmentainerFragment PageContaining(IReadOnlyList<FragmentainerFragment> fragmentainers, double y)
        {
            foreach (var fragmentainer in fragmentainers)
            {
                var bandTop = fragmentainer.Geometry.Top;
                if (y >= bandTop && y < bandTop + fragmentainer.Geometry.Height)
                    return fragmentainer;
            }
            return null;
        }

        #endregion
    }
}
