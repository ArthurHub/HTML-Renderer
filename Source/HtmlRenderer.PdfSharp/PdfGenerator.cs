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
using HtmlRenderer.Core;
using HtmlRenderer.Core.Entities;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace HtmlRenderer.PdfSharp
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public static class PdfGenerator
    {
        /// <summary>
        /// Create PDF document from given HTML.<br/>
        /// </summary>
        /// <param name="html">HTML source to create PDF from</param>
        /// <param name="pageSize">the page size to use for each page in the generated pdf </param>
        /// <param name="margin">the margin to use between the HTML and the edges of each page</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static PdfDocument GeneratePdf(string html, PageSize pageSize, int margin = 20, CssData cssData = null, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            var config = new PdfGenerateConfig();
            config.PageSize = pageSize;
            config.SetMargins(margin);
            return GeneratePdf(html, config, cssData, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Create PDF document from given HTML.<br/>
        /// </summary>
        /// <param name="html">HTML source to create PDF from</param>
        /// <param name="config">the configuration to use for the PDF generation (page size/page orientation/margins/etc.)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static PdfDocument GeneratePdf(string html, PdfGenerateConfig config, CssData cssData = null, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            // create PDF document to render the HTML into
            var document = new PdfDocument();
            
            // get the size of each page to layout the HTML in
            var size = PageSizeConverter.ToSize(config.PageSize);
            size = new XSize(size.Width - config.MarginLeft - config.MarginRight, size.Height - config.MarginTop - config.MarginBottom);

            if (!string.IsNullOrEmpty(html))
            {
                using (var container = new HtmlContainer())
                {
                    if (stylesheetLoad != null)
                        container.StylesheetLoad += stylesheetLoad;
                    if (imageLoad != null)
                        container.ImageLoad += imageLoad;

                    container.Location = new XPoint(config.MarginLeft, config.MarginTop);
                    container.MaxSize = new XSize(size.Width, 0);
                    container.SetHtml(html, cssData);

                    // layout the HTML with the page width restriction to know how many pages are required
                    using (var measure = XGraphics.CreateMeasureContext(size, XGraphicsUnit.Point, XPageDirection.Downwards))
                    {
                        container.PerformLayout(measure);
                    }

                    // while there is un-rendered HTML, create another PDF page and render with proper offset for the next page
                    double scrollOffset = 0;
                    while (scrollOffset > -container.ActualSize.Height)
                    {
                        var page = document.AddPage();
                        page.Size = config.PageSize;
                        using (var g = XGraphics.FromPdfPage(page))
                        {
                            g.IntersectClip(new XRect(config.MarginLeft, config.MarginTop, size.Width, size.Height));

                            container.ScrollOffset = new XPoint(0, scrollOffset);
                            container.PerformPaint(g);
                        }
                        scrollOffset -= size.Height;
                    }
                }
            }

            return document;
        }
    }
}