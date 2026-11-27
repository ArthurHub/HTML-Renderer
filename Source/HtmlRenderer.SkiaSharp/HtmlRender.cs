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
    /// Standalone static class for simple and direct HTML rendering with SkiaSharp.<br/>
    /// For low-level control and performance consider using <see cref="HtmlContainer"/>.<br/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Overwrite stylesheet resolution</b><br/>
    /// Exposed by optional "stylesheetLoad" delegate argument.<br/>
    /// Invoked when a stylesheet is about to be loaded by file path or URL in 'link' element.<br/>
    /// Allows to overwrite the loaded stylesheet by providing the stylesheet data manually, or different source (file or URL) to load from.<br/>
    /// If no alternative data is provided the original source will be used.<br/>
    /// </para>
    /// <para>
    /// <b>Overwrite image resolution</b><br/>
    /// Exposed by optional "imageLoad" delegate argument.<br/>
    /// Invoked when an image is about to be loaded by file path, URL or inline data in 'img' element or background-image CSS style.<br/>
    /// Allows to overwrite the loaded image by providing an <see cref="SKImage"/> or <see cref="SKBitmap"/>, or different source (file or URL) to load from.<br/>
    /// If no alternative data is provided the original source will be used.<br/>
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// <b>Simple rendering</b><br/>
    /// await HtmlRender.RenderAsync(canvas, "<![CDATA[<div>Hello <b>World</b></div>]]>");<br/>
    /// await HtmlRender.RenderAsync(canvas, "<![CDATA[<div>Hello <b>World</b></div>]]>", 10, 10, 500, CssData.Parse("body {font-size: 20px}")");<br/>
    /// </para>
    /// <para>
    /// <b>Image rendering</b><br/>
    /// using var image = await HtmlRender.RenderToImageAsync("<![CDATA[<div>Hello <b>World</b></div>]]>", new SKSizeI(600, 400));<br/>
    /// using var image = await HtmlRender.RenderToImageAsync("<![CDATA[<div>Hello <b>World</b></div>]]>", 600);<br/>
    /// </para>
    /// </example>
    public static class HtmlRender
    {
        private const int MaxUnlimitedWidth = 4096;

        // 4096 x 32768 RGBA are 512 MB
        private const int MaxUnlimitedHeight = 32768;

        /// <summary>
        /// Adds a typeface that isn't installed, e.g. loaded from a file, as a font family to be used in html rendering.<br/>
        /// The family is available by the typeface's family name.
        /// </summary>
        /// <param name="typeface">The typeface to add.</param>
        public static void AddFontFamily(SKTypeface typeface)
        {
            ArgChecker.AssertArgNotNull(typeface, "typeface");

            SkiaSharpAdapter.Instance.AddFontFamily(new FontFamilyAdapter(typeface));
        }

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
        /// <param name="stylesheet">the stylesheet source to parse</param>
        /// <param name="combineWithDefault">true - combine the parsed css data with default css data, false - return only the parsed css data</param>
        /// <returns>the parsed css data</returns>
        public static CssData ParseStyleSheet(string stylesheet, bool combineWithDefault = true)
        {
            return CssData.Parse(SkiaSharpAdapter.Instance, stylesheet, combineWithDefault);
        }

        /// <summary>
        /// Measure the size (width and height) required to draw the given html under given max width restriction.<br/>
        /// If no max width restriction is given the layout will use the maximum possible width required by the content,
        /// it can be the longest text line or full image width.
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">optional: bound the width of the html to render in (default - 0, unlimited)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the size required for the html</returns>
        public static async Task<SKSize> MeasureAsync(string html, float maxWidth = 0, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            if (string.IsNullOrEmpty(html))
                return SKSize.Empty;

            using (var container = CreateContainer(stylesheetLoad, imageLoad))
            {
                container.MaxSize = new SKSize(maxWidth, 0);
                await container.SetHtml(html, cssData).ConfigureAwait(false);
                container.PerformLayout();
                return container.ActualSize;
            }
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max width restriction.<br/>
        /// If <paramref name="maxWidth"/> is zero the html will use all the required width, otherwise it will perform line
        /// wrap as specified in the html<br/>
        /// Returned is the actual width and height of the rendered html.<br/>
        /// </summary>
        /// <param name="canvas">the canvas to draw on</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="left">optional: the left most location to start render the html at (default - 0)</param>
        /// <param name="top">optional: the top most location to start render the html at (default - 0)</param>
        /// <param name="maxWidth">optional: bound the width of the html to render in (default - 0, unlimited)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the actual size of the rendered html</returns>
        public static Task<SKSize> RenderAsync(SKCanvas canvas, string html, float left = 0, float top = 0, float maxWidth = 0, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            return RenderAsync(canvas, html, new SKPoint(left, top), new SKSize(maxWidth, 0), cssData, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.<br/>
        /// If <paramref name="maxSize"/>.Width is zero the html will use all the required width, otherwise it will perform line
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxSize"/>.Height is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// Returned is the actual width and height of the rendered html.<br/>
        /// </summary>
        /// <param name="canvas">the canvas to draw on</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="location">the top-left most location to start render the html at</param>
        /// <param name="maxSize">the max size of the rendered html (if height above zero it will be clipped)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the actual size of the rendered html</returns>
        public static async Task<SKSize> RenderAsync(SKCanvas canvas, string html, SKPoint location, SKSize maxSize, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(canvas, "canvas");

            if (string.IsNullOrEmpty(html))
                return SKSize.Empty;

            using (var container = CreateContainer(stylesheetLoad, imageLoad))
            {
                container.Location = location;
                container.MaxSize = maxSize;
                await container.SetHtml(html, cssData).ConfigureAwait(false);
                container.PerformLayout();

                var saveCount = canvas.Save();
                try
                {
                    if (maxSize.Height > 0)
                        canvas.ClipRect(SKRect.Create(location, maxSize));

                    container.PerformPaint(canvas);
                }
                finally
                {
                    canvas.RestoreToCount(saveCount);
                }

                return container.ActualSize;
            }
        }

        /// <summary>
        /// Renders the specified HTML into a new image of the requested size.<br/>
        /// The HTML will be layout by the given size but will be clipped if cannot fit.
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="size">The size of the image to render into, layout html by width and clipped by height</param>
        /// <param name="backgroundColor">optional: the color to fill the image with (default - white), can be transparent</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static async Task<SKImage> RenderToImageAsync(string html, SKSizeI size, SKColor? backgroundColor = null, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            using (var surface = CreateSurface(size))
            {
                var canvas = surface.Canvas;
                canvas.Clear(backgroundColor ?? SKColors.White);

                if (!string.IsNullOrEmpty(html))
                    await RenderAsync(canvas, html, SKPoint.Empty, size, cssData, stylesheetLoad, imageLoad).ConfigureAwait(false);

                return surface.Snapshot();
            }
        }

        /// <summary>
        /// Renders the specified HTML into a new image of unknown size that will be determined by max width/height and HTML layout.<br/>
        /// If <paramref name="maxWidth"/> is zero the html will use all the required width, otherwise it will perform line
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxHeight"/> is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">optional: the max width of the rendered html, if not zero and html cannot be layout within the limit it will be clipped</param>
        /// <param name="maxHeight">optional: the max height of the rendered html, if not zero and html cannot be layout within the limit it will be clipped</param>
        /// <param name="backgroundColor">optional: the color to fill the image with (default - white), can be transparent</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static Task<SKImage> RenderToImageAsync(string html, int maxWidth = 0, int maxHeight = 0, SKColor? backgroundColor = null, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            return RenderToImageAsync(html, SKSizeI.Empty, new SKSizeI(maxWidth, maxHeight), backgroundColor, cssData, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML into a new image of unknown size that will be determined by min/max width/height and HTML layout.<br/>
        /// If <paramref name="maxSize.Width"/> is zero the html will use all the required width, otherwise it will perform line
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxSize.Height"/> is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// If <paramref name="minSize"/> (Width/Height) is above zero the rendered image will not be smaller than the given min size.
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="minSize">optional: the min size of the rendered html (zero - not limit the width/height)</param>
        /// <param name="maxSize">optional: the max size of the rendered html, if not zero and html cannot be layout within the limit it will be clipped (zero - not limit the width/height)</param>
        /// <param name="backgroundColor">optional: the color to fill the image with (default - white), can be transparent</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static async Task<SKImage> RenderToImageAsync(string html, SKSizeI minSize, SKSizeI maxSize, SKColor? backgroundColor = null, CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            using (var container = CreateContainer(stylesheetLoad, imageLoad))
            {
                var finalSize = minSize;
                if (!string.IsNullOrEmpty(html))
                {
                    await container.SetHtml(html, cssData).ConfigureAwait(false);
                    finalSize = MeasureHtmlByRestrictions(container, minSize, maxSize);
                    container.MaxSize = finalSize;
                }

                using (var surface = CreateSurface(finalSize))
                {
                    var canvas = surface.Canvas;
                    canvas.Clear(backgroundColor ?? SKColors.White);

                    if (!string.IsNullOrEmpty(html))
                        container.PerformPaint(canvas);

                    return surface.Snapshot();
                }
            }
        }

        #region Private methods

        private static HtmlContainer CreateContainer(EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad, EventHandler<HtmlImageLoadEventArgs> imageLoad)
        {
            var container = new HtmlContainer();
            if (stylesheetLoad != null)
                container.StylesheetLoad += stylesheetLoad;
            if (imageLoad != null)
                container.ImageLoad += imageLoad;
            return container;
        }

        /// <summary>
        /// A raster surface, a zero size becomes a single pixel, Skia can't create an empty one.
        /// </summary>
        private static SKSurface CreateSurface(SKSizeI size)
        {
            var info = new SKImageInfo(Math.Max(size.Width, 1), Math.Max(size.Height, 1), SKColorType.Rgba8888, SKAlphaType.Premul);
            return SKSurface.Create(info)
                   ?? throw new InvalidOperationException($"Skia can't allocate an image of {info.Width}x{info.Height} pixels, limit the size with a max width and height.");
        }

        /// <summary>
        /// Measure the size of the html by performing layout under the given restrictions.
        /// </summary>
        private static SKSizeI MeasureHtmlByRestrictions(HtmlContainer htmlContainer, SKSizeI minSize, SKSizeI maxSize)
        {
            using (var g = new GraphicsAdapter())
            {
                var size = HtmlRendererUtils.MeasureHtmlByRestrictions(g, htmlContainer.HtmlContainerInt,
                    new RSize(minSize.Width, minSize.Height), new RSize(maxSize.Width, maxSize.Height));

                // Without a limit an unbreakable line or a long document could ask for a huge surface
                if (maxSize.Width < 1 && size.Width > MaxUnlimitedWidth)
                    size.Width = MaxUnlimitedWidth;
                if (maxSize.Height < 1 && size.Height > MaxUnlimitedHeight)
                    size.Height = MaxUnlimitedHeight;

                return Utils.ConvertRound(size);
            }
        }

        #endregion
    }
}
