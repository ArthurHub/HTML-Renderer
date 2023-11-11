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
using System;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public static class PdfGenerator
    {
        /// <summary>
        /// Adds a font mapping from <paramref name="fromFamily"/> to <paramref name="toFamily"/> iff the <paramref name="fromFamily"/> is not found.<br/>
        /// When the <paramref name="fromFamily"/> font is used in rendered html and is not found in existing 
        /// fonts (installed or added) it will be replaced by <paramref name="toFamily"/>.<br/>
        /// </summary>
        /// <remarks>
        /// This fonts mapping can be used as a fallback in case the requested font is not installed in the client system.
        /// </remarks>
        /// <param name="fromFamily">the font family to replace</param>
        /// <param name="toFamily">the font family to replace with</param>
        public static void AddFontFamilyMapping(string fromFamily, string toFamily)
        {
            ArgChecker.AssertArgNotNullOrEmpty(fromFamily, "fromFamily");
            ArgChecker.AssertArgNotNullOrEmpty(toFamily, "toFamily");

            SkiaSharpAdapter.Instance.AddFontFamilyMapping(fromFamily, toFamily);
        }

        /// <summary>
        /// Parse the given stylesheet to <see cref="CssData"/> object.<br/>
        /// If <paramref name="combineWithDefault"/> is true the parsed css blocks are added to the 
        /// default css data (as defined by W3), merged if class name already exists. If false only the data in the given stylesheet is returned.
        /// </summary>
        /// <seealso cref="http://www.w3.org/TR/CSS21/sample.html"/>
        /// <param name="stylesheet">the stylesheet source to parse</param>
        /// <param name="combineWithDefault">true - combine the parsed css data with default css data, false - return only the parsed css data</param>
        /// <returns>the parsed css data</returns>
        public static CssData ParseStyleSheet(string stylesheet, bool combineWithDefault = true)
        {
            return CssData.Parse(SkiaSharpAdapter.Instance, stylesheet, combineWithDefault);
        }

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
        public static async Task<SKDocument> GeneratePdfAsync(
            string html,
            Stream outputStream,
            int margin = 20, 
            CssData cssData = null, 
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, 
            EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {

            var config = new PdfGenerateConfig();
            config.PageSize = PageSize.A4;
            config.PageOrientation = PageOrientation.Portrait;

            return await GeneratePdfAsync(html, outputStream, config, cssData, stylesheetLoad, imageLoad);
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
        public static async Task<SKDocument> GeneratePdfAsync(
            string html,
            Stream outputStream,            
            PdfGenerateConfig config, 
            CssData cssData = null, 
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, 
            EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            // create PDF document to render the HTML into
            var document = SKDocument.CreatePdf(outputStream, 72f);

            // add rendered PDF pages to document
            await AddPdfPagesAsync(document, html, config, cssData, stylesheetLoad, imageLoad);

            return document;
        }

        /// <summary>
        /// Create PDF pages from given HTML and appends them to the provided PDF document.<br/>
        /// </summary>
        /// <param name="document">PDF document to append pages to</param>
        /// <param name="html">HTML source to create PDF from</param>
        /// <param name="pageSize">the page size to use for each page in the generated pdf </param>
        /// <param name="margin">the margin to use between the HTML and the edges of each page</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static async Task AddPdfPages(SKDocument document, string html, PageSize pageSize, int margin = 20, CssData cssData = null, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            var config = new PdfGenerateConfig();
            config.PageSize = pageSize;
            config.SetMargins(margin);
            await AddPdfPagesAsync(document, html, config, cssData, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Create PDF pages from given HTML and appends them to the provided PDF document.<br/>
        /// </summary>
        /// <param name="document">PDF document to append pages to</param>
        /// <param name="html">HTML source to create PDF from</param>
        /// <param name="config">the configuration to use for the PDF generation (page size/page orientation/margins/etc.)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static async Task AddPdfPagesAsync(
            SKDocument document, 
            string html, 
            PdfGenerateConfig config, 
            CssData cssData = null, 
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, 
            EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            SKSize orgPageSize;
            // get the size of each page to layout the HTML in
            if (config.PageSize == PageSize.A4)
                //8.3 x 11.7 inches in Portrait
                orgPageSize = new SKSize(8.3f * 72, 11.7f * 72);
            else
                orgPageSize = config.ManualPageSize;

            if (config.PageOrientation == PageOrientation.Landscape)
            {
                // invert pagesize for landscape
                orgPageSize = new SKSize(orgPageSize.Height, orgPageSize.Width);
            }

            var pageSize = new SKSize(orgPageSize.Width - config.MarginLeft - config.MarginRight, orgPageSize.Height - config.MarginTop - config.MarginBottom);

            if (!string.IsNullOrEmpty(html))
            {
                using (var container = new HtmlContainer())
                {
                    if (stylesheetLoad != null)
                        container.StylesheetLoad += stylesheetLoad;
                    if (imageLoad != null)
                        container.ImageLoad += imageLoad;

                    container.Location = new SKPoint(config.MarginLeft, config.MarginTop);
                    container.MaxSize = new SKSize(pageSize.Width, 0);
                    await container.SetHtml(html, cssData);
                    container.PageSize = pageSize;
                    container.MarginBottom = config.MarginBottom;
                    container.MarginLeft = config.MarginLeft;
                    container.MarginRight = config.MarginRight;
                    container.MarginTop = config.MarginTop;

                    // layout the HTML with the page width restriction to know how many pages are required
                    var docImageInfo = new SKImageInfo((int)orgPageSize.Width, (int)orgPageSize.Height * 7);
                    using (var s = SKSurface.Create(docImageInfo))
                    using (var g = s.Canvas)
                    {
                        await container.PerformLayout(g);

                        using (var image = s.Snapshot())
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
                        using (var stream = File.OpenWrite(Path.Combine(@"c:\temp\SkiaSharpTests", "layout.png")))
                        {
                            // save the data to a stream
                            data.SaveTo(stream);
                        }
                    }


                    // while there is un-rendered HTML, create another PDF page and render with proper offset for the next page
                    double scrollOffset = 0;
                    while (scrollOffset > -container.ActualSize.Height)
                    {
                        using (var page = document.BeginPage(orgPageSize.Width, orgPageSize.Height))
                        {
                            page.ClipRect(new SKRect(config.MarginLeft, config.MarginTop, pageSize.Width, pageSize.Height));

                            container.ScrollOffset = new SKPoint(0, (float)scrollOffset);

                            await container.PerformPaint(page);

                            document.EndPage();
                        }
                        scrollOffset -= pageSize.Height;
                    }

                    // add web links and anchors
                    HandleLinks(document, container, orgPageSize, pageSize);

                    document.Close();
                }
            }
        }



        #region Private/Protected methods

        /// <summary>
        /// Handle HTML links by create PDF Documents link either to external URL or to another page in the document.
        /// </summary>
        private static void HandleLinks(SKDocument document, HtmlContainer container, SKSize orgPageSize, SKSize pageSize)
        {
            /*
            foreach (var link in container.GetLinks())
            {
                int i = (int)(link.Rectangle.Top / pageSize.Height);
                for (; i < document.Pages.Count && pageSize.Height * i < link.Rectangle.Bottom; i++)
                {
                    var offset = pageSize.Height * i;

                    // fucking position is from the bottom of the page
                    var xRect = new XRect(link.Rectangle.Left, orgPageSize.Height - (link.Rectangle.Height + link.Rectangle.Top - offset), link.Rectangle.Width, link.Rectangle.Height);

                    if (link.IsAnchor)
                    {
                        // create link to another page in the document
                        var anchorRect = container.GetElementRectangle(link.AnchorId);
                        if (anchorRect.HasValue)
                        {
                            // document links to the same page as the link is not allowed
                            int anchorPageIdx = (int)(anchorRect.Value.Top / pageSize.Height);
                            if (i != anchorPageIdx)
                                document.Pages[i].AddDocumentLink(new PdfRectangle(xRect), anchorPageIdx);
                        }
                    }
                    else
                    {
                        // create link to URL
                        document.Pages[i].AddWebLink(new PdfRectangle(xRect), link.Href);
                    }
                }
            }
            */
        }

        #endregion
    }
}