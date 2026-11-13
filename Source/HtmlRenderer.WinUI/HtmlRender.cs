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
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.WinUI.Adapters;
using TheArtOfDev.HtmlRenderer.WinUI.Utilities;
using Windows.Foundation;
using Windows.UI;

namespace TheArtOfDev.HtmlRenderer.WinUI
{
    /// <summary>
    /// Standalone static class for simple and direct HTML rendering.<br/>
    /// For WinUI 3 UI prefer using HTML controls: <see cref="HtmlPanel"/> or <see cref="HtmlLabel"/>.<br/>
    /// For low-level control and performance consider using <see cref="HtmlContainer"/>.<br/>
    /// </summary>
    public static class HtmlRender
    {
        /// <summary>
        /// Adds a font family to be used in html rendering.<br/>
        /// The added font will be used by all rendering function including <see cref="HtmlContainer"/> and all WinUI 3 controls.
        /// </summary>
        /// <example>
        /// <code>HtmlRender.AddFontFamily(new Uri(@"C:\fonts\CustomFont.ttf"));</code>
        /// </example>
        /// <param name="fontFile">The font file with the font families to add, a file or <c>ms-appx:///</c> URI.</param>
        public static void AddFontFamily(Uri fontFile)
        {
            ArgChecker.AssertArgNotNull(fontFile, "fontFile");

            using (var fontSet = new CanvasFontSet(fontFile))
            {
                foreach (var font in fontSet.Fonts)
                {
                    var name = WinUIAdapter.GetPreferredFamilyName(font.FamilyNames);
                    if (name != null)
                        WinUIAdapter.Instance.AddFontFamily(new FontFamilyAdapter(name, fontFile));
                }
            }
        }

        /// <summary>
        /// Adds a font mapping from <paramref name="fromFamily"/> to <paramref name="toFamily"/> iff the <paramref name="fromFamily"/> is not found.<br/>
        /// </summary>
        /// <param name="fromFamily">the font family to replace</param>
        /// <param name="toFamily">the font family to replace with</param>
        public static void AddFontFamilyMapping(string fromFamily, string toFamily)
        {
            ArgChecker.AssertArgNotNullOrEmpty(fromFamily, "fromFamily");
            ArgChecker.AssertArgNotNullOrEmpty(toFamily, "toFamily");

            WinUIAdapter.Instance.AddFontFamilyMapping(fromFamily, toFamily);
        }

        /// <summary>
        /// Parse the given stylesheet to <see cref="CssData"/> object.<br/>
        /// </summary>
        /// <param name="stylesheet">the stylesheet source to parse</param>
        /// <param name="combineWithDefault">true - combine the parsed css data with default css data, false - return only the parsed css data</param>
        /// <returns>the parsed css data</returns>
        public static CssData ParseStyleSheet(string stylesheet, bool combineWithDefault = true)
        {
            return CssData.Parse(WinUIAdapter.Instance, stylesheet, combineWithDefault);
        }

        /// <summary>
        /// Measure the size (width and height) required to draw the given html under given max width restriction.<br/>
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">optional: bound the width of the html to render in (default - 0, unlimited)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the size required for the html</returns>
        public static async Task<Size> MeasureAsync(string html, double maxWidth = 0, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            Size actualSize = default;
            if (!string.IsNullOrEmpty(html))
            {
                using (var container = new HtmlContainer())
                {
                    container.MaxSize = new Size(maxWidth, 0);
                    container.AvoidAsyncImagesLoading = true;
                    container.AvoidImagesLateLoading = true;

                    if (stylesheetLoad != null)
                        container.StylesheetLoad += stylesheetLoad;
                    if (imageLoad != null)
                        container.ImageLoad += imageLoad;

                    await container.SetHtml(html, cssData).ConfigureAwait(false);
                    container.PerformLayout();

                    actualSize = container.ActualSize;
                }
            }
            return actualSize;
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max width restriction.<br/>
        /// </summary>
        /// <param name="g">Device to render with</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="left">optional: the left most location to start render the html at (default - 0)</param>
        /// <param name="top">optional: the top most location to start render the html at (default - 0)</param>
        /// <param name="maxWidth">optional: bound the width of the html to render in (default - 0, unlimited)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the actual size of the rendered html</returns>
        public static Task<Size> RenderAsync(CanvasDrawingSession g, string html, double left = 0, double top = 0, double maxWidth = 0, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            return RenderClip(g, html, new Point(left, top), new Size(maxWidth, 0), cssData, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.<br/>
        /// </summary>
        /// <param name="g">Device to render with</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="location">the top-left most location to start render the html at</param>
        /// <param name="maxSize">the max size of the rendered html (if height above zero it will be clipped)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the actual size of the rendered html</returns>
        public static Task<Size> RenderAsync(CanvasDrawingSession g, string html, Point location, Size maxSize, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            return RenderClip(g, html, location, maxSize, cssData, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML into a new image of the requested size.<br/>
        /// The HTML will be layout by the given size but will be clipped if cannot fit.<br/>
        /// </summary>
        /// <remarks>
        /// Doesn't need the UI thread.
        /// </remarks>
        /// <param name="html">HTML source to render</param>
        /// <param name="size">The size of the image to render into, layout html by width and clipped by height</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static async Task<CanvasRenderTarget> RenderToImageAsync(string html, Size size, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            var renderTarget = new CanvasRenderTarget(WinUIAdapter.Instance.Device, (float)Math.Max(1, size.Width), (float)Math.Max(1, size.Height), 96);

            if (!string.IsNullOrEmpty(html))
            {
                using (var g = renderTarget.CreateDrawingSession())
                {
                    g.Clear(Colors.White);
                    await RenderHtml(g, html, new Point(), size, cssData, stylesheetLoad, imageLoad).ConfigureAwait(false);
                }
            }

            return renderTarget;
        }

        /// <summary>
        /// Renders the specified HTML into a new image of unknown size that will be determined by max width/height and HTML layout.<br/>
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">optional: the max width of the rendered html, if not zero and html cannot be layout within the limit it will be clipped</param>
        /// <param name="maxHeight">optional: the max height of the rendered html, if not zero and html cannot be layout within the limit it will be clipped</param>
        /// <param name="backgroundColor">optional: the color to fill the image with (default - white)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static Task<CanvasRenderTarget> RenderToImageAsync(string html, int maxWidth = 0, int maxHeight = 0, Color backgroundColor = default,
            CssData cssData = null, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            return RenderToImageAsync(html, default, new Size(maxWidth, maxHeight), backgroundColor, cssData, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML into a new image of unknown size that will be determined by min/max width/height and HTML layout.<br/>
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="minSize">optional: the min size of the rendered html (zero - not limit the width/height)</param>
        /// <param name="maxSize">optional: the max size of the rendered html, if not zero and html cannot be layout within the limit it will be clipped (zero - not limit the width/height)</param>
        /// <param name="backgroundColor">optional: the color to fill the image with (default - white)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static async Task<CanvasRenderTarget> RenderToImageAsync(string html, Size minSize, Size maxSize, Color backgroundColor = default, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            CanvasRenderTarget renderTarget;
            if (!string.IsNullOrEmpty(html))
            {
                using (var container = new HtmlContainer())
                {
                    container.AvoidAsyncImagesLoading = true;
                    container.AvoidImagesLateLoading = true;

                    if (stylesheetLoad != null)
                        container.StylesheetLoad += stylesheetLoad;
                    if (imageLoad != null)
                        container.ImageLoad += imageLoad;
                    await container.SetHtml(html, cssData).ConfigureAwait(false);

                    var finalSize = MeasureHtmlByRestrictions(container, minSize, maxSize);
                    container.MaxSize = finalSize;

                    renderTarget = new CanvasRenderTarget(WinUIAdapter.Instance.Device, (float)Math.Max(1, finalSize.Width), (float)Math.Max(1, finalSize.Height), 96);

                    using (var g = renderTarget.CreateDrawingSession())
                    {
                        g.Clear(backgroundColor.Equals(default(Color)) ? Colors.White : backgroundColor);
                        container.PerformPaint(g, new Rect(0, 0, maxSize.Width > 0 ? maxSize.Width : double.MaxValue, maxSize.Height > 0 ? maxSize.Height : double.MaxValue));
                    }
                }
            }
            else
            {
                renderTarget = new CanvasRenderTarget(WinUIAdapter.Instance.Device, 1, 1, 96);
            }

            return renderTarget;
        }

        #region Private methods

        /// <summary>
        /// Measure the size of the html by performing layout under the given restrictions.
        /// </summary>
        private static Size MeasureHtmlByRestrictions(HtmlContainer htmlContainer, Size minSize, Size maxSize)
        {
            using (var mg = new GraphicsAdapter())
            {
                var sizeInt = HtmlRendererUtils.MeasureHtmlByRestrictions(mg, htmlContainer.HtmlContainerInt, Utils.Convert(minSize), Utils.Convert(maxSize));
                if (maxSize.Width < 1 && sizeInt.Width > 4096)
                    sizeInt.Width = 4096;
                return Utils.ConvertRound(sizeInt);
            }
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction, clipping
        /// the graphics so the html will not be rendered outside the max height bound given.
        /// </summary>
        private static async Task<Size> RenderClip(CanvasDrawingSession g, string html, Point location, Size maxSize, CssData cssData, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad, EventHandler<HtmlImageLoadEventArgs> imageLoad)
        {
            if (maxSize.Height > 0)
            {
                using (g.CreateLayer(1f, new Rect(location, maxSize)))
                {
                    return await RenderHtml(g, html, location, maxSize, cssData, stylesheetLoad, imageLoad).ConfigureAwait(false);
                }
            }

            return await RenderHtml(g, html, location, maxSize, cssData, stylesheetLoad, imageLoad).ConfigureAwait(false);
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.
        /// </summary>
        private static async Task<Size> RenderHtml(CanvasDrawingSession g, string html, Point location, Size maxSize, CssData cssData, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad, EventHandler<HtmlImageLoadEventArgs> imageLoad)
        {
            Size actualSize = default;

            if (!string.IsNullOrEmpty(html))
            {
                using (var container = new HtmlContainer())
                {
                    container.Location = location;
                    container.MaxSize = maxSize;
                    container.AvoidAsyncImagesLoading = true;
                    container.AvoidImagesLateLoading = true;

                    if (stylesheetLoad != null)
                        container.StylesheetLoad += stylesheetLoad;
                    if (imageLoad != null)
                        container.ImageLoad += imageLoad;

                    await container.SetHtml(html, cssData).ConfigureAwait(false);
                    container.PerformLayout();
                    container.PerformPaint(g, new Rect(0, 0, double.MaxValue, double.MaxValue));

                    actualSize = container.ActualSize;
                }
            }

            return actualSize;
        }

        #endregion
    }
}
