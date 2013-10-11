// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they bagin again,
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
    /// Static class for direct html rendering, intendent for small html fragments.<br/>
    /// Prefer using WinForms Html controls: <see cref="HtmlPanel"/> or <see cref="HtmlLabel"/>.
    /// For direct non-trivial html rendering consider using <see cref="HtmlContainer"/>.
    /// </summary>
    /// <remarks>
    /// Not suitable for large htmls as each render call will parse the given html string into DOM structure, for large html it
    /// can be very expensive.<br/>
    /// Consider using <see cref="HtmlContainer"/> for large html or for performance.
    /// </remarks>
    /// <example>
    /// HtmlRender.Render(g, "<![CDATA[<div>Hello <b>World</b></div>]]>");<br/>
    /// HtmlRender.Render(g, "<![CDATA[<div>Hello <b>World</b></div>]]>", 10, 10, 500, CssData.Parse("body {font-size: 20px}")");<br/>
    /// </example>
    public static class HtmlRender
    {
        /// <summary>
        /// Adds a font family to be used in html rendering.<br/>
        /// The added font will be used by all rendering function including <see cref="HtmlContainer"/> and all winforms controls.
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
        /// </summary>
        /// <param name="g">Device to use for measure</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">optional: bound the width of the html to render in (default - 0, unlimited)</param>
        /// <param name="cssData">optiona: the style to use for html rendering (default - use W3 default style)</param>
        /// <returns>the size required for the html</returns>
        public static SizeF Measure(Graphics g, string html, float maxWidth = 0, CssData cssData = null)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            return Measure(g, html, maxWidth, cssData, null, null);
        }

        /// <summary>
        /// Measure the size (width and height) required to draw the given html under given width and height restrictions.<br/>
        /// </summary>
        /// <param name="g">Device to use for measure</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">optional: bound the width of the html to render in (default - 0, unlimited)</param>
        /// <param name="cssData">optiona: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the size required for the html</returns>
        public static SizeF Measure(Graphics g, string html, float maxWidth, CssData cssData, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad, EventHandler<HtmlImageLoadEventArgs> imageLoad)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            SizeF actualSize = SizeF.Empty;
            if (!string.IsNullOrEmpty(html))
            {
                using (var container = new HtmlContainer())
                {
                    if (stylesheetLoad != null)
                        container.StylesheetLoad += stylesheetLoad;
                    if (imageLoad != null)
                        container.ImageLoad += imageLoad;
                    container.SetHtml(html, cssData);
                    container.MaxSize = new SizeF(maxWidth, 0);
                    container.PerformLayout(g);
                    actualSize = container.ActualSize;
                }
            }
            return actualSize;
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.<br/>
        /// If <paramref name="maxWidth"/> is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// Returned is the actual widht and height of the rendered html.<br/>
        /// </summary>
        /// <param name="g">Device to render with</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="left">optional: the left most location to start render the html at (default - 0)</param>
        /// <param name="top">optional: the top most location to start render the html at (default - 0)</param>
        /// <param name="maxWidth">optional: Width to fit HTML drawing (default - 0, unlimited)</param>
        /// <param name="cssData">optiona: the style to use for html rendering (default - use W3 default style)</param>
        /// <returns>the actual size of the rendered html</returns>
        public static SizeF Render(Graphics g, string html, float left = 0, float top = 0, float maxWidth = 0, CssData cssData = null)
        {
            return Render(g, html, new PointF(left, top), new SizeF(maxWidth, 0), cssData, null, null);
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.<br/>
        /// If <paramref name="maxSize"/>.Width is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxSize"/>.Height is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// Returned is the actual widht and height of the rendered html.<br/>
        /// </summary>
        /// <param name="g">Device to render with</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="location">the top-left most location to start render the html at</param>
        /// <param name="maxSize">the max size of the rendered html (if height above zero it will be clipped)</param>
        /// <param name="cssData">optiona: the style to use for html rendering (default - use W3 default style)</param>
        /// <returns>the actual size of the rendered html</returns>
        public static SizeF Render(Graphics g, string html, PointF location, SizeF maxSize, CssData cssData = null)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            return Render(g, html, location, maxSize, cssData, null, null);
        }

        /// <summary>
        /// Renders the specified HTML source on the specified location and max size restriction.<br/>
        /// If <paramref name="maxSize"/>.Width is zero the html will use all the required width, otherwise it will perform line 
        /// wrap as specified in the html<br/>
        /// If <paramref name="maxSize"/>.Height is zero the html will use all the required height, otherwise it will clip at the
        /// given max height not rendering the html below it.<br/>
        /// Returned is the actual widht and height of the rendered html.<br/>
        /// </summary>
        /// <param name="g">Device to render with</param>
        /// <param name="html">HTML source to render</param>
        /// <param name="location">the top-left most location to start render the html at</param>
        /// <param name="maxSize">the max size of the rendered html (if height above zero it will be clipped)</param>
        /// <param name="cssData">optiona: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the actual size of the rendered html</returns>
        public static SizeF Render(Graphics g, string html, PointF location, SizeF maxSize, CssData cssData, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad, EventHandler<HtmlImageLoadEventArgs> imageLoad)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            SizeF actualSize = SizeF.Empty;
            if (!string.IsNullOrEmpty(html))
            {
                Region prevClip = null;
                if (maxSize.Height > 0)
                {
                    prevClip = g.Clip;
                    g.SetClip(new RectangleF(location, maxSize));
                }

                using (var container = new HtmlContainer())
                {
                    if (stylesheetLoad != null)
                        container.StylesheetLoad += stylesheetLoad;
                    if (imageLoad != null)
                        container.ImageLoad += imageLoad;
                    container.SetHtml(html, cssData);
                    container.Location = location;
                    container.MaxSize = maxSize;
                    container.PerformLayout(g);
                    container.PerformPaint(g);
                    
                    if (prevClip != null)
                    {
                        g.SetClip(prevClip, CombineMode.Replace);
                    }

                    actualSize = container.ActualSize;
                }
            }

            return actualSize;
        }

        /// <summary>
        /// Renders the specified HTML into image of the requested size.<br/>
        /// The HTML will be layout by the given size but will be clipped if cannot fit.
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="size">The size of the image to render into, layout html by width and clipped by height</param>
        /// <param name="cssData">optiona: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static Image RenderToImage(string html, Size size, CssData cssData = null, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            // create the final image to render into
            var image = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);

            if (!string.IsNullOrEmpty(html))
            {
                // create memory buffer from desktop handle that supports alpha chanel
                IntPtr dib;
                var memoryHdc = Win32Utils.CreateMemoryHdc(IntPtr.Zero, image.Width, image.Height, out dib);
                try
                {
                    // create memory buffer graphics to use for HTML rendering
                    using (var memoryGraphics = Graphics.FromHdc(memoryHdc))
                    {
                        memoryGraphics.Clear(Color.White);

                        // render HTML into the memory buffer
                        using (var htmlContainer = new HtmlContainer())
                        {
                            if (stylesheetLoad != null)
                                htmlContainer.StylesheetLoad += stylesheetLoad;
                            if (imageLoad != null)
                                htmlContainer.ImageLoad += imageLoad;
                            htmlContainer.SetHtml(html, cssData);
                            htmlContainer.MaxSize = size;
                            htmlContainer.PerformLayout(memoryGraphics);
                            htmlContainer.PerformPaint(memoryGraphics);
                        }
                    }

                    // copy from memory buffer to image
                    using (var imageGraphics = Graphics.FromImage(image))
                    {
                        var imgHdc = imageGraphics.GetHdc();
                        Win32Utils.BitBlt(imgHdc, 0, 0, image.Width, image.Height, memoryHdc, 0, 0, Win32Utils.BitBltCopy);
                        imageGraphics.ReleaseHdc(imgHdc);
                    }
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
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">the max width of the rendered html</param>
        /// <param name="maxHeight">optional: the max height of the rendered html, if above zero it will be clipped</param>
        /// <param name="cssData">optiona: the style to use for html rendering (default - use W3 default style)</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated image of the html</returns>
        public static Image RenderToImage(string html, int maxWidth, int maxHeight = 0, CssData cssData = null, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            if (string.IsNullOrEmpty(html))
                return new Bitmap(0, 0, PixelFormat.Format32bppArgb);

            using (var htmlContainer = new HtmlContainer())
            {
                // use desktop created graphics to measure the HTML
                using (var measureGraphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    if (stylesheetLoad != null)
                        htmlContainer.StylesheetLoad += stylesheetLoad;
                    if (imageLoad != null)
                        htmlContainer.ImageLoad += imageLoad;
                    htmlContainer.SetHtml(html, cssData);

                    htmlContainer.PerformLayout(measureGraphics);

                    if (maxWidth > 0 && maxWidth < htmlContainer.ActualSize.Width)
                    {
                        // to allow the actual size be smaller than max we need to set max size only if it is really larger
                        htmlContainer.MaxSize = new SizeF(maxWidth, 0);
                        htmlContainer.PerformLayout(measureGraphics);
                    }
                }

                var size = new Size(maxWidth > 0 ? Math.Min(maxWidth, (int) htmlContainer.ActualSize.Width) : (int) htmlContainer.ActualSize.Width,
                                    maxHeight > 0 ? Math.Min(maxHeight, (int) htmlContainer.ActualSize.Height) : (int) htmlContainer.ActualSize.Height);

                // create the final image to render into by measured size
                var image = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);

                // create memory buffer from desktop handle that supports alpha chanel
                IntPtr dib;
                var memoryHdc = Win32Utils.CreateMemoryHdc(IntPtr.Zero, image.Width, image.Height, out dib);
                try
                {
                    // render HTML into the memory buffer
                    using (var memoryGraphics = Graphics.FromHdc(memoryHdc))
                    {
                        memoryGraphics.Clear(Color.White);
                        htmlContainer.PerformPaint(memoryGraphics);
                    }

                    // copy from memory buffer to image
                    using (var imageGraphics = Graphics.FromImage(image))
                    {
                        var imgHdc = imageGraphics.GetHdc();
                        Win32Utils.BitBlt(imgHdc, 0, 0, image.Width, image.Height, memoryHdc, 0, 0, Win32Utils.BitBltCopy);
                        imageGraphics.ReleaseHdc(imgHdc);
                    }
                }
                finally
                {
                    Win32Utils.ReleaseMemoryHdc(memoryHdc, dib);
                }

                return image;
            }
        }
    }
}
