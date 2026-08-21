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

using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.PdfSharp.Adapters;

namespace TheArtOfDev.HtmlRenderer.PdfSharp
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public static class PdfGenerator
    {
        /// <summary>
        /// Adds a font mapping from <paramref name="fromFamily"/> to <paramref name="toFamily"/> iff the <paramref name="fromFamily"/> is not found.<br/>
        /// When the <paramref name="fromFamily"/> font is used in rendered HTML and is not found in existing 
        /// fonts (installed or added) it will be replaced by <paramref name="toFamily"/>.<br/>
        /// </summary>
        /// <remarks>
        /// This font mapping can be used as a fallback in case the requested font is not installed in the client system.
        /// </remarks>
        /// <param name="fromFamily">the font family to replace</param>
        /// <param name="toFamily">the font family to replace with</param>
        public static void AddFontFamilyMapping(string fromFamily, string toFamily)
        {
            ArgChecker.AssertArgNotNullOrEmpty(fromFamily, "fromFamily");
            ArgChecker.AssertArgNotNullOrEmpty(toFamily, "toFamily");

            PdfSharpAdapter.Instance.AddFontFamilyMapping(fromFamily, toFamily);
        }

        /// <summary>
        /// Registers a custom directory to search for font files (TTF/OTF).<br/>
        /// Fonts in this directory will be discovered and made available for rendering.
        /// </summary>
        /// <remarks>
        /// This will automatically trigger a rediscovery of all fonts.
        /// </remarks>
        /// <param name="fontDirectory">the directory path containing font files</param>
        public static void RegisterCustomFontDirectory(string fontDirectory)
        {
            ArgChecker.AssertArgNotNullOrEmpty(fontDirectory, "fontDirectory");

            PdfSharpAdapter.Instance.FontResolver.RegisterCustomFontDirectory(fontDirectory);
            
            // Trigger a rediscovery
            var fontFamilies = PdfSharpAdapter.Instance.FontResolver.DiscoverFontFamilies();
            
            foreach (var fontFamily in fontFamilies)
            {
                PdfSharpAdapter.Instance.AddFontFamily(new FontFamilyAdapter(new XFontFamily(fontFamily)));
            }
        }

        /// <summary>
        /// Parse the given stylesheet to <see cref="CssData"/> object.<br/>
        /// If <paramref name="combineWithDefault"/> is true the parsed CSS blocks are added to the 
        /// default CSS data (as defined by W3), merged if class name already exists. If false only the data in the given stylesheet is returned.
        /// </summary>
        /// <seealso cref="http://www.w3.org/TR/CSS21/sample.html"/>
        /// <param name="stylesheet">the stylesheet source to parse</param>
        /// <param name="combineWithDefault">true - combine the parsed CSS data with default CSS data, false - return only the parsed CSS data</param>
        /// <returns>the parsed CSS data</returns>
        public static Task<CssData> ParseStyleSheet(string stylesheet, bool combineWithDefault = true)
        {
            // CssData.Parse -> CssParser.ParseStyleSheet is not itself async yet (its @import resolution
            // bridges into the async resource-loading pipeline synchronously for now - it's also called
            // directly from several UI controls' public API, out of scope for this conversion) - wrapped
            // in Task.FromResult here so this method's own shape matches the rest of this async-only API.
            return Task.FromResult(CssData.Parse(PdfSharpAdapter.Instance, stylesheet, combineWithDefault));
        }

        /// <summary>
        /// Create PDF document from given HTML.<br/>
        /// </summary>
        /// <param name="html">HTML source to create PDF from</param>
        /// <param name="pageSize">the page size to use for each page in the generated pdf </param>
        /// <param name="margin">the margin to use between the HTML and the edges of each page</param>
        /// <param name="cssData">optional: the style to use for HTML rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the HTML</returns>
        public static async Task<PdfDocument> GeneratePdf(string html, PageSize pageSize, int margin = 20, CssData cssData = null, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            var config = new PdfGenerateConfig();
            config.PageSize = pageSize;
            config.SetMargins(margin);
            return await GeneratePdf(html, config, cssData, stylesheetLoad, imageLoad).ConfigureAwait(false);
        }

        /// <summary>
        /// Create PDF document from given HTML.<br/>
        /// </summary>
        /// <param name="html">HTML source to create PDF from</param>
        /// <param name="config">the configuration to use for the PDF generation (page size/page orientation/margins/etc.)</param>
        /// <param name="cssData">optional: the style to use for HTML rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the HTML</returns>
        public static async Task<PdfDocument> GeneratePdf(string html, PdfGenerateConfig config, CssData cssData = null, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            // create PDF document to render the HTML into
            var document = new PdfDocument();

            // add rendered PDF pages to document
            await AddPdfPages(document, html, config, cssData, stylesheetLoad, imageLoad).ConfigureAwait(false);

            return document;
        }

        /// <summary>
        /// Create PDF pages from given HTML and appends them to the provided PDF document.<br/>
        /// </summary>
        /// <param name="document">PDF document to append pages to</param>
        /// <param name="html">HTML source to create PDF from</param>
        /// <param name="pageSize">the page size to use for each page in the generated pdf </param>
        /// <param name="margin">the margin to use between the HTML and the edges of each page</param>
        /// <param name="cssData">optional: the style to use for HTML rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the HTML</returns>
        public static Task AddPdfPages(PdfDocument document, string html, PageSize pageSize, int margin = 20, CssData cssData = null, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            var config = new PdfGenerateConfig();
            config.PageSize = pageSize;
            config.SetMargins(margin);
            return AddPdfPages(document, html, config, cssData, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Create PDF pages from given HTML and appends them to the provided PDF document.<br/>
        /// </summary>
        /// <param name="document">PDF document to append pages to</param>
        /// <param name="html">HTML source to create PDF from</param>
        /// <param name="config">the configuration to use for the PDF generation (page size/page orientation/margins/etc.)</param>
        /// <param name="cssData">optional: the style to use for HTML rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the HTML</returns>
        public static async Task AddPdfPages(PdfDocument document, string html, PdfGenerateConfig config, CssData cssData = null, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            XSize orgPageSize;
            // get the size of each page to layout the HTML in
            if (config.PageSize != PageSize.Undefined)
                orgPageSize = PageSizeConverter.ToSize(config.PageSize);
            else
                orgPageSize = config.ManualPageSize;

            if (config.PageOrientation == PageOrientation.Landscape)
            {
                // invert pagesize for landscape
                orgPageSize = new XSize(orgPageSize.Height, orgPageSize.Width);
            }

            var pageSize = new XSize(orgPageSize.Width - config.MarginLeft - config.MarginRight, orgPageSize.Height - config.MarginTop - config.MarginBottom);

            if (!string.IsNullOrEmpty(html))
            {
                using (var container = new HtmlContainer())
                {
                    if (stylesheetLoad != null)
                        container.StylesheetLoad += stylesheetLoad;
                    if (imageLoad != null)
                        container.ImageLoad += imageLoad;

                    container.Location = new XPoint(config.MarginLeft, config.MarginTop);
                    container.MaxSize = new XSize(pageSize.Width, 0);
                    await container.SetHtml(html, cssData).ConfigureAwait(false);
                    container.PageSize = pageSize;
                    container.MarginBottom = config.MarginBottom;
                    container.MarginLeft = config.MarginLeft;
                    container.MarginRight = config.MarginRight;
                    container.MarginTop = config.MarginTop;

                    // layout the HTML with the page width restriction to know how many pages are required
                    using (var measure = XGraphics.CreateMeasureContext(pageSize, XGraphicsUnit.Point, XPageDirection.Downwards))
                    {
                        container.PerformLayout(measure);
                    }

                    // One PDF page per fragmentainer the fragment tree actually materialized - a
                    // content-empty page slot (CSS Paged Media 3 3.2, e.g. a huge margin that would
                    // otherwise paginate through blank vertical space - see the margin-truncation
                    // correction in BlockFragmentation) is simply never in this list, which is what
                    // gives blank-page skipping for free here instead of the old ceil(height/pageHeight)
                    // loop's naive page count.
                    var tree = container.FragmentTree;
                    foreach (var fragmentainer in tree?.Fragmentainers ?? (IReadOnlyList<FragmentainerFragment>)Array.Empty<FragmentainerFragment>())
                    {
                        var page = document.AddPage();
                        page.Height = XUnit.FromPoint(orgPageSize.Height);
                        page.Width = XUnit.FromPoint(orgPageSize.Width);
                        page.Orientation = config.PageOrientation;

                        using (var g = XGraphics.FromPdfPage(page))
                        {
                            g.IntersectClip(new XRect(0, 0, page.Width.Point, page.Height.Point));

                            container.PerformPaint(g, fragmentainer);
                        }
                    }

                    // add web links and anchors
                    HandleLinks(document, container, orgPageSize, tree);
                }
            }
        }



        #region Private/Protected methods

        /// <summary>
        /// Handle HTML links by create PDF Documents link either to external URL or to another page in the document.
        /// </summary>
        private static void HandleLinks(PdfDocument document, HtmlContainer container, XSize orgPageSize, FragmentTree tree)
        {
            if (tree == null || tree.Fragmentainers.Count == 0)
                return;

            // Pagination slot -> PDF page index. Not a bare multiply/divide by page height any more:
            // a content-empty slot is never materialized as a fragmentainer at all (blank-page
            // skipping), so slot indices are not contiguous across tree.Fragmentainers the way a
            // fixed-size page grid's would be.
            var slotToPage = new Dictionary<int, int>();
            for (var pageIndex = 0; pageIndex < tree.Fragmentainers.Count; pageIndex++)
            {
                slotToPage[tree.Fragmentainers[pageIndex].SlotIndex] = pageIndex;
            }

            foreach (var link in container.GetLinks())
            {
                foreach (var fragmentainer in tree.Fragmentainers)
                {
                    var bandTop = fragmentainer.Geometry.Top;
                    var bandBottom = bandTop + fragmentainer.Geometry.Height;
                    if (link.Rectangle.Top >= bandBottom || link.Rectangle.Bottom <= bandTop)
                        continue; // this link has no part on this fragmentainer's page

                    var pageIndex = slotToPage[fragmentainer.SlotIndex];

                    // fucking position is from the bottom of the page
                    var xRect = new XRect(link.Rectangle.Left, orgPageSize.Height - (link.Rectangle.Height + link.Rectangle.Top - bandTop), link.Rectangle.Width, link.Rectangle.Height);

                    if (link.IsAnchor)
                    {
                        // create link to another page in the document
                        var anchorRect = container.GetElementRectangle(link.AnchorId);
                        if (anchorRect.HasValue)
                        {
                            var anchorSlot = SlotContaining(tree, anchorRect.Value.Top);
                            // document links to the same page as the link is not allowed
                            if (anchorSlot.HasValue && slotToPage.TryGetValue(anchorSlot.Value, out var anchorPageIdx) && pageIndex != anchorPageIdx)
                            {
                                document.Pages[pageIndex].AddDocumentLink(new PdfRectangle(xRect), anchorPageIdx);
                            }
                        }
                    }
                    else
                    {
                        // create link to URL
                        document.Pages[pageIndex].AddWebLink(new PdfRectangle(xRect), link.Href);
                    }
                }
            }
        }

        /// <summary>
        /// The pagination slot whose content band contains document-space Y coordinate <paramref name="y"/>,
        /// or null if it falls in no materialized fragmentainer's band (e.g. an anchor inside a
        /// content-empty page slot that was skipped, or past the end of the document).
        /// </summary>
        private static int? SlotContaining(FragmentTree tree, double y)
        {
            foreach (var fragmentainer in tree.Fragmentainers)
            {
                var bandTop = fragmentainer.Geometry.Top;
                if (y >= bandTop && y < bandTop + fragmentainer.Geometry.Height)
                    return fragmentainer.SlotIndex;
            }
            return null;
        }

        #endregion
    }
}
