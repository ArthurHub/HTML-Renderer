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
using System.Threading.Tasks;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters;
using TheArtOfDev.HtmlRenderer.SkiaSharp.Utilities;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp
{
    /// <summary>
    /// Low level handling of Html Renderer logic, for drawing onto your own <see cref="SKCanvas"/>.<br/>
    /// For simply rendering a document or an image use <see cref="HtmlRender"/>.
    /// </summary>
    /// <seealso cref="HtmlContainerInt"/>
    public sealed class HtmlContainer : IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// The internal core html container
        /// </summary>
        private readonly HtmlContainerInt _htmlContainerInt;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public HtmlContainer()
        {
            _htmlContainerInt = new HtmlContainerInt(SkiaSharpAdapter.Instance);
            _htmlContainerInt.SetMargins(0);
            // One continuous page, painting clips to it
            _htmlContainerInt.PageSize = new RSize(99999, 99999);
            _htmlContainerInt.AvoidAsyncImagesLoading = true;
            _htmlContainerInt.AvoidImagesLateLoading = true;
        }

        /// <summary>
        /// Raised when the set html document has been fully loaded.<br/>
        /// Allows manipulation of the html dom, scroll position, etc.
        /// </summary>
        public event EventHandler LoadComplete
        {
            add { _htmlContainerInt.LoadComplete += value; }
            remove { _htmlContainerInt.LoadComplete -= value; }
        }

        /// <summary>
        /// Raised when an error occurred during html rendering.<br/>
        /// </summary>
        /// <remarks>
        /// There is no guarantee that the event will be raised on the main thread, it can be raised on thread-pool thread.
        /// </remarks>
        public event EventHandler<HtmlRenderErrorEventArgs> RenderError
        {
            add { _htmlContainerInt.RenderError += value; }
            remove { _htmlContainerInt.RenderError -= value; }
        }

        /// <summary>
        /// Raised when a stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or Uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad
        {
            add { _htmlContainerInt.StylesheetLoad += value; }
            remove { _htmlContainerInt.StylesheetLoad -= value; }
        }

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually as an <see cref="SKImage"/> or <see cref="SKBitmap"/>,
        /// if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad
        {
            add { _htmlContainerInt.ImageLoad += value; }
            remove { _htmlContainerInt.ImageLoad -= value; }
        }

        /// <summary>
        /// The internal core html container
        /// </summary>
        internal HtmlContainerInt HtmlContainerInt
        {
            get { return _htmlContainerInt; }
        }

        /// <summary>
        /// the parsed stylesheet data used for handling the html
        /// </summary>
        public CssData CssData
        {
            get { return _htmlContainerInt.CssData; }
        }

        /// <summary>
        /// Gets or sets a value indicating if anti-aliasing should be avoided for geometry like backgrounds and borders (default - false).
        /// </summary>
        public bool AvoidGeometryAntialias
        {
            get { return _htmlContainerInt.AvoidGeometryAntialias; }
            set { _htmlContainerInt.AvoidGeometryAntialias = value; }
        }

        /// <summary>
        /// The scroll offset of the html.<br/>
        /// This will adjust the rendered html by the given offset so the content will be "scrolled".<br/>
        /// </summary>
        /// <example>
        /// Element that is rendered at location (50,100) with offset of (0,200) will not be rendered as it
        /// will be at -100 therefore outside the client rectangle.
        /// </example>
        public SKPoint ScrollOffset
        {
            get { return Utils.Convert(_htmlContainerInt.ScrollOffset); }
            set { _htmlContainerInt.ScrollOffset = Utils.Convert(value); }
        }

        /// <summary>
        /// The top-left most location of the rendered html.<br/>
        /// This will offset the top-left corner of the rendered html.
        /// </summary>
        public SKPoint Location
        {
            get { return Utils.Convert(_htmlContainerInt.Location); }
            set { _htmlContainerInt.Location = Utils.Convert(value); }
        }

        /// <summary>
        /// The max width and height of the rendered html.<br/>
        /// The max width will effect the html layout wrapping lines, resize images and tables where possible.<br/>
        /// The max height does NOT effect layout, but will not render outside it (clip).<br/>
        /// <see cref="ActualSize"/> can be exceed the max size by layout restrictions (unwrappable line, set image size, etc.).<br/>
        /// Set zero for unlimited (width\height separately).<br/>
        /// </summary>
        public SKSize MaxSize
        {
            get { return Utils.Convert(_htmlContainerInt.MaxSize); }
            set { _htmlContainerInt.MaxSize = Utils.Convert(value); }
        }

        /// <summary>
        /// The CSS media type <c>@media</c> rules are evaluated against, <c>"screen"</c> by default.
        /// Set it before <see cref="SetHtml"/>.
        /// </summary>
        public string MediaType
        {
            get { return _htmlContainerInt.MediaType ?? SkiaSharpAdapter.Instance.DefaultMediaType; }
            set { _htmlContainerInt.MediaType = value; }
        }

        /// <summary>
        /// The actual size of the rendered html (after layout)
        /// </summary>
        public SKSize ActualSize
        {
            get { return Utils.Convert(_htmlContainerInt.ActualSize); }
        }

        /// <summary>
        /// Get the currently selected text segment in the html.
        /// </summary>
        public string SelectedText
        {
            get { return _htmlContainerInt.SelectedText; }
        }

        /// <summary>
        /// Copy the currently selected html segment with style.
        /// </summary>
        public string SelectedHtml
        {
            get { return _htmlContainerInt.SelectedHtml; }
        }

        /// <summary>
        /// Init with optional document and stylesheet.
        /// </summary>
        /// <param name="htmlSource">the html to init with, init empty if not given</param>
        /// <param name="baseCssData">optional: the stylesheet to init with, init default if not given</param>
        public Task SetHtml(string htmlSource, CssData baseCssData = null)
        {
            return _htmlContainerInt.SetHtml(htmlSource, baseCssData);
        }

        /// <summary>
        /// Get html from the current DOM tree with style if requested.
        /// </summary>
        /// <param name="styleGen">Optional: controls the way styles are generated when html is generated (default: <see cref="HtmlGenerationStyle.Inline"/>)</param>
        /// <returns>generated html</returns>
        public string GetHtml(HtmlGenerationStyle styleGen = HtmlGenerationStyle.Inline)
        {
            return _htmlContainerInt.GetHtml(styleGen);
        }

        /// <summary>
        /// Get attribute value of element at the given x,y location by given key.<br/>
        /// If more than one element exist with the attribute at the location the inner most is returned.
        /// </summary>
        /// <param name="location">the location to find the attribute at</param>
        /// <param name="attribute">the attribute key to get value by</param>
        /// <returns>found attribute value or null if not found</returns>
        public string GetAttributeAt(SKPoint location, string attribute)
        {
            return _htmlContainerInt.GetAttributeAt(Utils.Convert(location), attribute);
        }

        /// <summary>
        /// Get all the links in the HTML with the element rectangle and href data.
        /// </summary>
        /// <returns>collection of all the links in the HTML</returns>
        public List<LinkElementData<SKRect>> GetLinks()
        {
            var linkElements = new List<LinkElementData<SKRect>>();
            foreach (var link in HtmlContainerInt.GetLinks())
            {
                linkElements.Add(new LinkElementData<SKRect>(link.Id, link.Href, Utils.Convert(link.Rectangle)));
            }
            return linkElements;
        }

        /// <summary>
        /// Get css link href at the given x,y location.
        /// </summary>
        /// <param name="location">the location to find the link at</param>
        /// <returns>css link href if exists or null</returns>
        public string GetLinkAt(SKPoint location)
        {
            return _htmlContainerInt.GetLinkAt(Utils.Convert(location));
        }

        /// <summary>
        /// Get the rectangle of html element as calculated by html layout.<br/>
        /// Element if found by id (id attribute on the html element).<br/>
        /// Note: to get the screen rectangle you need to adjust by the hosting control.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to get its rectangle</param>
        /// <returns>the rectangle of the element or null if not found</returns>
        public SKRect? GetElementRectangle(string elementId)
        {
            var r = _htmlContainerInt.GetElementRectangle(elementId);
            return r.HasValue ? Utils.Convert(r.Value) : (SKRect?)null;
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.
        /// </summary>
        /// <remarks>
        /// Skia measures text without a canvas, so the layout doesn't depend on where it's drawn later.
        /// </remarks>
        public void PerformLayout()
        {
            using (var ig = new GraphicsAdapter())
            {
                _htmlContainerInt.PerformLayout(ig);
            }
        }

        /// <summary>
        /// Render the html onto the given canvas.
        /// </summary>
        /// <param name="canvas">the canvas to draw on</param>
        public void PerformPaint(SKCanvas canvas)
        {
            ArgChecker.AssertArgNotNull(canvas, "canvas");

            var clip = canvas.LocalClipBounds;
            using (var ig = new GraphicsAdapter(canvas, Utils.Convert(clip)))
            {
                _htmlContainerInt.PerformPaint(ig);
            }
        }

        /// <summary>
        /// The fragment tree of the last <see cref="PerformLayout"/>, one fragmentainer per page.
        /// </summary>
        internal Core.Fragments.FragmentTree FragmentTree
        {
            get { return _htmlContainerInt.FragmentTree; }
        }

        /// <summary>
        /// Render one page onto the given canvas.
        /// </summary>
        internal void PerformPaint(SKCanvas canvas, Core.Fragments.FragmentainerFragment fragmentainer)
        {
            var clip = canvas.LocalClipBounds;
            using (var ig = new GraphicsAdapter(canvas, Utils.Convert(clip)))
            {
                _htmlContainerInt.PerformPaint(ig, fragmentainer);
            }
        }

        public void Dispose()
        {
            _htmlContainerInt.Dispose();
        }
    }
}
