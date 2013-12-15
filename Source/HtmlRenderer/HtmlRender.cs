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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using HtmlRenderer.Entities;
using HtmlRenderer.Utils;

namespace HtmlRenderer
{
    /// <summary>
    /// Standalone static class for simple and direct HTML rendering.<br/>
    /// For WinForms UI prefer using HTML controls: <see cref="HtmlPanel"/> or <see cref="HtmlLabel"/>.<br/>
    /// For low-level control and performance consider using <see cref="HtmlContainer"/>.<br/>
    /// </summary>
    /// <remarks>
    /// <b>GDI vs. GDI+ text rendering</b><br/>
    /// Windows supports two text rendering technologies: GDI and GDI+.<br/> 
    /// GDI is older, has better performance and looks better on standard monitors but doesn't support alpha channel for transparency.<br/> 
    /// GDI+ is newer, device independent so work better for printers but is slower and looks worse on monitors.<br/>
    /// HtmlRender supports both GDI and GDI+ text rendering to acc
    /// </remarks>
    /// <example>
    /// <para>
    /// <b>Simple rendering</b><br/>
    /// HtmlRender.Render(g, "<![CDATA[<div>Hello <b>World</b></div>]]>");<br/>
    /// HtmlRender.Render(g, "<![CDATA[<div>Hello <b>World</b></div>]]>", 10, 10, 500, CssData.Parse("body {font-size: 20px}")");<br/>
    /// </para>
    /// <para>
    /// <b>Image rendering</b><br/>
    /// </para>
    /// </example>
    public static class HtmlRender
    {
        /// <summary>
        /// Adds a font family to be used in html rendering.<br/>
        /// The added font will be used by all rendering function including <see cref="HtmlContainer"/> and all WinForms controls.
        /// </summary>
        /// <remarks>
        /// The given font family instance must be remain alive while the renderer is in use.<br/>
        /// If loaded to <see cref="PrivateFontCollection"/> then the collection must be alive.<br/>
        /// If loaded from file then the file must not be deleted.
        /// </remarks>
        /// <param name="fontFamily">The font family to add.</param>
        public static void AddFontFamily(FontFamily fontFamily)
        {
            ArgChecker.AssertArgNotNull(fontFamily, "fontFamily");

            FontsUtils.AddFontFamily(fontFamily);
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

            FontsUtils.AddFontFamilyMapping(fromFamily, toFamily);
        }

        /// <summary>
        /// Measure the size (width and height) required to draw the given html under given width and height restrictions.<br/>
        /// Use GDI text rendering, note <see cref="Graphics.TextRenderingHint"/> has no effect.
        /// </summary>
        /// <param name="g">Device to use for measure</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">optional: bound the width of the html to render in (default - 0, unlimited)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the size required for the html</returns>
        public static SizeF Measure(Graphics g, string html, float maxWidth, CssData cssData = null,
                                    EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            return Measure(g, html, maxWidth, cssData, false, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Measure the size (width and height) required to draw the given html under given width and height restrictions.<br/>
        /// Use GDI+ text rending, use <see cref="Graphics.TextRenderingHint"/> to control text rendering.
        /// </summary>
        /// <param name="g">Device to use for measure</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">optional: bound the width of the html to render in (default - 0, unlimited)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the size required for the html</returns>
        public static SizeF MeasureGdiPlus(Graphics g, string html, float maxWidth, CssData cssData = null,
                                           EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            return Measure(g, html, maxWidth, cssData, true, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.<br/>
        /// Use GDI text rendering, note <see cref="Graphics.TextRenderingHint"/> has no effect.
        /// If <paramref name="maxWidth"/> is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// Returned is the actual width and height of the rendered html.<br/>
        /// </summary>
        /// <param name="g">Device to render with</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="left">optional: the left most location to start render the html at (default - 0)</param>
        /// <param name="top">optional: the top most location to start render the html at (default - 0)</param>
        /// <param name="maxWidth">optional: Width to fit HTML drawing (default - 0, unlimited)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the actual size of the rendered html</returns>
        public static SizeF Render(Graphics g, string html, float left = 0, float top = 0, float maxWidth = 0, CssData cssData = null,
                                   EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            return RenderClip(g, html, new PointF(left, top), new SizeF(maxWidth, 0), cssData, false, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.<br/>
        /// Use GDI text rendering, note <see cref="Graphics.TextRenderingHint"/> has no effect.
        /// If <paramref name="maxSize"/>.Width is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxSize"/>.Height is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// Returned is the actual width and height of the rendered html.<br/>
        /// </summary>
        /// <param name="g">Device to render with</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="location">the top-left most location to start render the html at</param>
        /// <param name="maxSize">the max size of the rendered html (if height above zero it will be clipped)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the actual size of the rendered html</returns>
        public static SizeF Render(Graphics g, string html, PointF location, SizeF maxSize, CssData cssData = null,
                                   EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            return RenderClip(g, html, location, maxSize, cssData, false, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.<br/>
        /// Use GDI+ text rending, use <see cref="Graphics.TextRenderingHint"/> to control text rendering.
        /// If <paramref name="maxWidth"/> is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// Returned is the actual width and height of the rendered html.<br/>
        /// </summary>
        /// <param name="g">Device to render with</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="left">optional: the left most location to start render the html at (default - 0)</param>
        /// <param name="top">optional: the top most location to start render the html at (default - 0)</param>
        /// <param name="maxWidth">optional: Width to fit HTML drawing (default - 0, unlimited)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the actual size of the rendered html</returns>
        public static SizeF RenderGdiPlus(Graphics g, string html, float left = 0, float top = 0, float maxWidth = 0, CssData cssData = null,
                                          EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            return RenderClip(g, html, new PointF(left, top), new SizeF(maxWidth, 0), cssData, true, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.<br/>
        /// Use GDI+ text rending, use <see cref="Graphics.TextRenderingHint"/> to control text rendering.
        /// If <paramref name="maxSize"/>.Width is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxSize"/>.Height is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// Returned is the actual width and height of the rendered html.<br/>
        /// </summary>
        /// <param name="g">Device to render with</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="location">the top-left most location to start render the html at</param>
        /// <param name="maxSize">the max size of the rendered html (if height above zero it will be clipped)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the actual size of the rendered html</returns>
        public static SizeF RenderGdiPlus(Graphics g, string html, PointF location, SizeF maxSize, CssData cssData = null,
                                          EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            return RenderClip(g, html, location, maxSize, cssData, true, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML on top of the given image.<br/>
        /// <paramref name="image"/> will contain the rendered html in it on top of original content.<br/>
        /// The HTML will be layout by the given image size but may be clipped if cannot fit.<br/>
        /// </summary>
        /// <param name="image">the image to render the html on</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="location">the top-left most location to start render the html at</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        public static void RenderToImage(Image image, string html, PointF location, CssData cssData = null,
                                         EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(image, "image");
            var maxSize = new SizeF(image.Size.Width - location.X, image.Size.Height - location.Y);
            RenderToImage(image, html, location, maxSize, cssData, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML on top of the given image.<br/>
        /// <paramref name="image"/> will contain the rendered html in it on top of original content.<br/>
        /// </summary>
        /// <param name="image">the image to render the html on</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="location">the top-left most location to start render the html at</param>
        /// <param name="maxSize">the max size of the rendered html (if height above zero it will be clipped)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        public static void RenderToImage(Image image, string html, PointF location, SizeF maxSize, CssData cssData = null,
                                         EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNull(image, "image");

            if( !string.IsNullOrEmpty(html) )
            {
                // create memory buffer from desktop handle that supports alpha channel
                IntPtr dib;
                var memoryHdc = Win32Utils.CreateMemoryHdc(IntPtr.Zero, image.Width, image.Height, out dib);
                try
                {
                    // create memory buffer graphics to use for HTML rendering
                    using(var memoryGraphics = Graphics.FromHdc(memoryHdc))
                    {
                        // draw the image to the memory buffer to be the background of the rendered html
                        memoryGraphics.DrawImageUnscaled(image, 0, 0);

                        // render HTML into the memory buffer
                        RenderHtml(memoryGraphics, html, location, maxSize, cssData, false, stylesheetLoad, imageLoad);
                    }

                    // copy from memory buffer to image
                    CopyBufferToImage(memoryHdc, image);
                }
                finally
                {
                    Win32Utils.ReleaseMemoryHdc(memoryHdc, dib);
                }
            }
        }

        /// <summary>
        /// Renders the specified HTML into image of the requested size.<br/>
        /// The HTML will be layout by the given size but will be clipped if cannot fit.<br/>
        /// <p>
        /// Limitation: The image cannot have transparent background, by default it will be white.
        /// </p>
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="size">The size of the image to render into, layout html by width and clipped by height</param>
        /// <param name="backgroundColor">optional: the color to fill the image with (default - white)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="backgroundColor"/> is <see cref="Color.Transparent"/></exception>.
        public static Image RenderToImage(string html, Size size, Color backgroundColor = new Color(), CssData cssData = null,
                                          EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            if( backgroundColor == Color.Transparent )
                throw new ArgumentOutOfRangeException("backgroundColor", "Transparent background in not supported");

            // create the final image to render into
            var image = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);

            if( !string.IsNullOrEmpty(html) )
            {
                // create memory buffer from desktop handle that supports alpha channel
                IntPtr dib;
                var memoryHdc = Win32Utils.CreateMemoryHdc(IntPtr.Zero, image.Width, image.Height, out dib);
                try
                {
                    // create memory buffer graphics to use for HTML rendering
                    using(var memoryGraphics = Graphics.FromHdc(memoryHdc))
                    {
                        memoryGraphics.Clear(backgroundColor != Color.Empty ? backgroundColor : Color.White);

                        // render HTML into the memory buffer
                        RenderHtml(memoryGraphics, html, PointF.Empty, size, cssData, true, stylesheetLoad, imageLoad);
                    }

                    // copy from memory buffer to image
                    CopyBufferToImage(memoryHdc, image);
                }
                finally
                {
                    Win32Utils.ReleaseMemoryHdc(memoryHdc, dib);
                }
            }

            return image;
        }

        /// <summary>
        /// Renders the specified HTML into image of unknown size that will be determined by max width/height and HTML layout.<br/>
        /// If <paramref name="maxWidth"/> is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxHeight"/> is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// <p>
        /// Limitation: The image cannot have transparent background, by default it will be white.
        /// </p>
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">the max width of the rendered html, if not zero and html cannot be layout within the limit it will be clipped</param>
        /// <param name="maxHeight">optional: the max height of the rendered html, if not zero and html cannot be layout within the limit it will be clipped</param>
        /// <param name="backgroundColor">optional: the color to fill the image with (default - white)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="backgroundColor"/> is <see cref="Color.Transparent"/></exception>.
        public static Image RenderToImage(string html, int maxWidth, int maxHeight = 0, Color backgroundColor = new Color(), CssData cssData = null,
                                          EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            return RenderToImage(html, Size.Empty, new Size(maxWidth, maxHeight), backgroundColor, cssData, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML into image of unknown size that will be determined by max width/height and HTML layout.<br/>
        /// If <paramref name="maxSize.Width"/> is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxSize.Height"/> is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// If <paramref name="minSize"/> (Width/Height) is above zero the rendered image will not be smaller than the given min size.<br/>
        /// <p>
        /// Limitation: The image cannot have transparent background, by default it will be white.
        /// </p>
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="minSize">optional: the min size of the rendered html (zero - not limit the width/height)</param>
        /// <param name="maxSize">optional: the max size of the rendered html, if not zero and html cannot be layout within the limit it will be clipped (zero - not limit the width/height)</param>
        /// <param name="backgroundColor">optional: the color to fill the image with (default - white)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="backgroundColor"/> is <see cref="Color.Transparent"/></exception>.
        public static Image RenderToImage(string html, Size minSize, Size maxSize, Color backgroundColor = new Color(), CssData cssData = null,
                                          EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            if( backgroundColor == Color.Transparent )
                throw new ArgumentOutOfRangeException("backgroundColor", "Transparent background in not supported");

            if( string.IsNullOrEmpty(html) )
                return new Bitmap(0, 0, PixelFormat.Format32bppArgb);

            using(var htmlContainer = new HtmlContainer())
            {
                if (stylesheetLoad != null)
                    htmlContainer.StylesheetLoad += stylesheetLoad;
                if (imageLoad != null)
                    htmlContainer.ImageLoad += imageLoad;
                htmlContainer.SetHtml(html, cssData);

                var finalSize = MeasureHtmlByRestrictions(htmlContainer, minSize, maxSize);
                htmlContainer.MaxSize = finalSize;
                
                // create the final image to render into by measured size
                var image = new Bitmap(finalSize.Width, finalSize.Height, PixelFormat.Format32bppArgb);

                // create memory buffer from desktop handle that supports alpha channel
                IntPtr dib;
                var memoryHdc = Win32Utils.CreateMemoryHdc(IntPtr.Zero, image.Width, image.Height, out dib);
                try
                {
                    // render HTML into the memory buffer
                    using(var memoryGraphics = Graphics.FromHdc(memoryHdc))
                    {
                        memoryGraphics.Clear(backgroundColor != Color.Empty ? backgroundColor : Color.White);
                        htmlContainer.PerformPaint(memoryGraphics);
                    }

                    // copy from memory buffer to image
                    CopyBufferToImage(memoryHdc, image);
                }
                finally
                {
                    Win32Utils.ReleaseMemoryHdc(memoryHdc, dib);
                }

                return image;
            }
        }

        /// <summary>
        /// Renders the specified HTML into image of the requested size.<br/>
        /// The HTML will be layout by the given size but will be clipped if cannot fit.<br/>
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="size">The size of the image to render into, layout html by width and clipped by height</param>
        /// <param name="textRenderingHint">optional: (default - SingleBitPerPixelGridFit)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static Image RenderToImageGdiPlus(string html, Size size, TextRenderingHint textRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit, CssData cssData = null,
                                              EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            var image = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);

            using(var g = Graphics.FromImage(image))
            {
                g.TextRenderingHint = textRenderingHint;
                RenderHtml(g, html, PointF.Empty, size, cssData, true, stylesheetLoad, imageLoad);
            }

            return image;
        }

        /// <summary>
        /// Renders the specified HTML into image of unknown size that will be determined by max width/height and HTML layout.<br/>
        /// If <paramref name="maxWidth"/> is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxHeight"/> is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// <p>
        /// Limitation: The image cannot have transparent background, by default it will be white.
        /// </p>
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">the max width of the rendered html, if not zero and html cannot be layout within the limit it will be clipped</param>
        /// <param name="maxHeight">optional: the max height of the rendered html, if not zero and html cannot be layout within the limit it will be clipped</param>
        /// <param name="textRenderingHint">optional: (default - SingleBitPerPixelGridFit)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static Image RenderToImageGdiPlus(string html, int maxWidth, int maxHeight = 0, TextRenderingHint textRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit, CssData cssData = null,
                                          EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            return RenderToImageGdiPlus(html, Size.Empty, new Size(maxWidth, maxHeight), textRenderingHint, cssData, stylesheetLoad, imageLoad);
        }

        /// <summary>
        /// Renders the specified HTML into image of unknown size that will be determined by max width/height and HTML layout.<br/>
        /// If <paramref name="maxSize.Width"/> is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxSize.Height"/> is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// If <paramref name="minSize"/> (Width/Height) is above zero the rendered image will not be smaller than the given min size.<br/>
        /// <p>
        /// Limitation: The image cannot have transparent background, by default it will be white.
        /// </p>
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="minSize">optional: the min size of the rendered html (zero - not limit the width/height)</param>
        /// <param name="maxSize">optional: the max size of the rendered html, if not zero and html cannot be layout within the limit it will be clipped (zero - not limit the width/height)</param>
        /// <param name="textRenderingHint">optional: (default - SingleBitPerPixelGridFit)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static Image RenderToImageGdiPlus(string html, Size minSize, Size maxSize, TextRenderingHint textRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit, CssData cssData = null,
                                          EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            if (string.IsNullOrEmpty(html))
                return new Bitmap(0, 0, PixelFormat.Format32bppArgb);

            using(var htmlContainer = new HtmlContainer())
            {
                if( stylesheetLoad != null )
                    htmlContainer.StylesheetLoad += stylesheetLoad;
                if( imageLoad != null )
                    htmlContainer.ImageLoad += imageLoad;
                htmlContainer.SetHtml(html, cssData);

                var finalSize = MeasureHtmlByRestrictions(htmlContainer, minSize, maxSize);
                htmlContainer.MaxSize = finalSize;

                // create the final image to render into by measured size
                var image = new Bitmap(finalSize.Width, finalSize.Height, PixelFormat.Format32bppArgb);
                
                // render HTML into the image
                using (var g = Graphics.FromImage(image))
                {
                    g.TextRenderingHint = textRenderingHint;
                    htmlContainer.PerformPaint(g);
                }

                return image;
            }
        }


        #region Private methods

        /// <summary>
        /// Measure the size (width and height) required to draw the given html under given width and height restrictions.<br/>
        /// </summary>
        /// <param name="g">Device to use for measure</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">optional: bound the width of the html to render in (default - 0, unlimited)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="useGdiPlusTextRendering">true - use GDI+ text rendering, false - use GDI text rendering</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the size required for the html</returns>
        private static SizeF Measure(Graphics g, string html, float maxWidth, CssData cssData, bool useGdiPlusTextRendering, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad, EventHandler<HtmlImageLoadEventArgs> imageLoad)
        {
            SizeF actualSize = SizeF.Empty;
            if( !string.IsNullOrEmpty(html) )
            {
                using(var container = new HtmlContainer())
                {
                    container.MaxSize = new SizeF(maxWidth, 0);
                    container.UseGdiPlusTextRendering = useGdiPlusTextRendering;

                    if( stylesheetLoad != null )
                        container.StylesheetLoad += stylesheetLoad;
                    if( imageLoad != null )
                        container.ImageLoad += imageLoad;

                    container.SetHtml(html, cssData);
                    container.PerformLayout(g);

                    actualSize = container.ActualSize;
                }
            }
            return actualSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="htmlContainer">the html to calculate the layout for</param>
        /// <param name="minSize">the minimal size of the rendered html (zero - not limit the width/height)</param>
        /// <param name="maxSize">the maximum size of the rendered html, if not zero and html cannot be layout within the limit it will be clipped (zero - not limit the width/height)</param>
        /// <returns>return: the size of the html to be rendered within the min/max limits</returns>
        private static Size MeasureHtmlByRestrictions(HtmlContainer htmlContainer, Size minSize, Size maxSize)
        {
            // use desktop created graphics to measure the HTML
            using (var measureGraphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                // first layout without size restriction to know html actual size
                htmlContainer.PerformLayout(measureGraphics);

                if (maxSize.Width > 0 && maxSize.Width < htmlContainer.ActualSize.Width)
                {
                    // to allow the actual size be smaller than max we need to set max size only if it is really larger
                    htmlContainer.MaxSize = new SizeF(maxSize.Width, 0);
                    htmlContainer.PerformLayout(measureGraphics);
                }
            }

            // restrict the final size by min/max
            var finalWidth = Math.Max(maxSize.Width > 0 ? Math.Min(maxSize.Width, (int)htmlContainer.ActualSize.Width) : (int)htmlContainer.ActualSize.Width, minSize.Width);
            var finalHeight = Math.Max(maxSize.Height > 0 ? Math.Min(maxSize.Height, (int)htmlContainer.ActualSize.Height) : (int)htmlContainer.ActualSize.Height, minSize.Height);

            return new Size(finalWidth, finalHeight);
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.<br/>
        /// If <paramref name="maxSize"/>.Width is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxSize"/>.Height is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// Clip the graphics so the html will not be rendered outside the max height bound given.<br/>
        /// Returned is the actual width and height of the rendered html.<br/>
        /// </summary>
        /// <param name="g">Device to render with</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="location">the top-left most location to start render the html at</param>
        /// <param name="maxSize">the max size of the rendered html (if height above zero it will be clipped)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="useGdiPlusTextRendering">true - use GDI+ text rendering, false - use GDI text rendering</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the actual size of the rendered html</returns>
        private static SizeF RenderClip(Graphics g, string html, PointF location, SizeF maxSize, CssData cssData, bool useGdiPlusTextRendering, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad, EventHandler<HtmlImageLoadEventArgs> imageLoad)
        {
            Region prevClip = null;
            if( maxSize.Height > 0 )
            {
                prevClip = g.Clip;
                g.SetClip(new RectangleF(location, maxSize));
            }

            var actualSize = RenderHtml(g, html, location, maxSize, cssData, useGdiPlusTextRendering, stylesheetLoad, imageLoad);

            if( prevClip != null )
            {
                g.SetClip(prevClip, CombineMode.Replace);
            }

            return actualSize;
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.<br/>
        /// If <paramref name="maxSize"/>.Width is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxSize"/>.Height is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// Returned is the actual width and height of the rendered html.<br/>
        /// </summary>
        /// <param name="g">Device to render with</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="location">the top-left most location to start render the html at</param>
        /// <param name="maxSize">the max size of the rendered html (if height above zero it will be clipped)</param>
        /// <param name="cssData">optional: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="useGdiPlusTextRendering">true - use GDI+ text rendering, false - use GDI text rendering</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the actual size of the rendered html</returns>
        private static SizeF RenderHtml(Graphics g, string html, PointF location, SizeF maxSize, CssData cssData, bool useGdiPlusTextRendering, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad, EventHandler<HtmlImageLoadEventArgs> imageLoad)
        {
            SizeF actualSize = SizeF.Empty;

            if( !string.IsNullOrEmpty(html) )
            {
                using(var container = new HtmlContainer())
                {
                    container.Location = location;
                    container.MaxSize = maxSize;
                    container.UseGdiPlusTextRendering = useGdiPlusTextRendering;

                    if( stylesheetLoad != null )
                        container.StylesheetLoad += stylesheetLoad;
                    if( imageLoad != null )
                        container.ImageLoad += imageLoad;

                    container.SetHtml(html, cssData);
                    container.PerformLayout(g);
                    container.PerformPaint(g);

                    actualSize = container.ActualSize;
                }
            }

            return actualSize;
        }

        /// <summary>
        /// Copy all the bitmap bits from memory bitmap buffer to the given image.
        /// </summary>
        /// <param name="memoryHdc">the source memory bitmap buffer to copy from</param>
        /// <param name="image">the destination bitmap image to copy to</param>
        private static void CopyBufferToImage(IntPtr memoryHdc, Image image)
        {
            using(var imageGraphics = Graphics.FromImage(image))
            {
                var imgHdc = imageGraphics.GetHdc();
                Win32Utils.BitBlt(imgHdc, 0, 0, image.Width, image.Height, memoryHdc, 0, 0, Win32Utils.BitBltCopy);
                imageGraphics.ReleaseHdc(imgHdc);
            }
        }

        #endregion
    }
}
