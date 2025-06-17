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

using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;

namespace TheArtOfDev.HtmlRenderer.Demo.WinForms
{
    internal static class HtmlRenderingHelper
    {
        #region Fields/Consts

        /// <summary>
        /// Cache for resource images
        /// </summary>
        private static readonly Dictionary<string, Image> _imageCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Cache for PDF resource images
        /// </summary>
        private static readonly Dictionary<string, XImage> _pdfImageCache = new Dictionary<string, XImage>(StringComparer.OrdinalIgnoreCase);

        #endregion

        /// <summary>
        /// Check if currently running in mono.
        /// </summary>
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        /// <summary>
        /// Create image to be used to fill background so it will be clear that what's on top is transparent.
        /// </summary>
        public static Bitmap CreateImageForTransparentBackground()
        {
            var image = new Bitmap(10, 10);
            using (var g = Graphics.FromImage(image))
            {
                g.Clear(Color.White);
                g.FillRectangle(SystemBrushes.Control, new Rectangle(0, 0, 5, 5));
                g.FillRectangle(SystemBrushes.Control, new Rectangle(5, 5, 5, 5));
            }
            return image;
        }

        /// <summary>
        /// Get image by resource key.
        /// </summary>
        public static Image TryLoadResourceImage(string src)
        {
            if (string.IsNullOrEmpty(src))
                return null;

            Image image;
            if (!_imageCache.TryGetValue(src, out image))
            {
                try
                {
                    var imageStream = DemoUtils.GetImageStream(src);
                    if (imageStream != null)
                    {
                        image = Image.FromStream(imageStream);
                        _imageCache[src] = image;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load image '{src}': {ex.Message}");
                    return null;
                }
            }
            return image;
        }

        /// <summary>
        /// Get XImage by resource key safely for PDF generation.
        /// </summary>
        public static XImage TryLoadResourceXImage(string src)
        {
            if (string.IsNullOrEmpty(src))
                return null;

            // Check if we already have this image cached
            if (_pdfImageCache.TryGetValue(src, out var cachedXImg))
                return cachedXImg;

            try
            {
                var img = TryLoadResourceImage(src);
                if (img == null)
                    return null;

                XImage xImg = null;

                // Convert to PNG format first to ensure compatibility with PdfSharp
                using (var tempBmp = new Bitmap(img))
                using (var ms = new MemoryStream())
                {
                    tempBmp.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    xImg = XImage.FromStream(ms);

                    // Cache the successfully loaded XImage
                    _pdfImageCache[src] = xImg;
                }

                return xImg;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to convert image '{src}' to XImage: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// On image load in renderer set the image by event async.
        /// </summary>
        public static void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
        {
            ImageLoad(e, false);
        }

        /// <summary>
        /// On image load in renderer set the image by event async.
        /// </summary>
        public static void OnImageLoadPdfSharp(object sender, HtmlImageLoadEventArgs e)
        {
            ImageLoad(e, true);
        }

        /// <summary>
        /// On image load in renderer set the image by event async.
        /// </summary>
        public static void ImageLoad(HtmlImageLoadEventArgs e, bool pdfSharp)
        {
            object imgObj = null;

            try
            {
                // Handle special case attributes
                if (!e.Handled && e.Attributes != null)
                {
                    if (e.Attributes.ContainsKey("byevent"))
                    {
                        if (Int32.TryParse(e.Attributes["byevent"], out int delay))
                        {
                            e.Handled = true;
                            ThreadPool.QueueUserWorkItem(state =>
                            {
                                Thread.Sleep(delay);
                                try
                                {
                                    e.Callback("https://fbcdn-sphotos-a-a.akamaihd.net/hphotos-ak-snc7/c0.44.403.403/p403x403/318890_10151195988833836_1081776452_n.jpg");
                                }
                                catch { /* Ignore callback errors */ }
                            });
                            return;
                        }
                        else
                        {
                            e.Callback("http://sphotos-a.xx.fbcdn.net/hphotos-ash4/c22.0.403.403/p403x403/263440_10152243591765596_773620816_n.jpg");
                            return;
                        }
                    }
                    else if (e.Attributes.ContainsKey("byrect"))
                    {
                        try
                        {
                            var split = e.Attributes["byrect"].Split(',');
                            var rect = new Rectangle(Int32.Parse(split[0]), Int32.Parse(split[1]), Int32.Parse(split[2]), Int32.Parse(split[3]));

                            imgObj = null;
                            if (pdfSharp)
                            {
                                imgObj = TryLoadResourceXImage(src: "htmlicon");
                            }
                            else
                            {
                                imgObj = TryLoadResourceImage(src: "htmlicon");
                            }

                            e.Callback(imgObj, rect.X, rect.Y, rect.Width, rect.Height);
                            return;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing 'byrect' attribute: {ex.Message}");
                            // Continue with normal processing if rect parsing fails
                        }
                    }
                }

                // Standard image loading
                imgObj = null;

                if (pdfSharp)
                {
                    imgObj = TryLoadResourceXImage(e.Src);
                }
                else
                {
                    imgObj = TryLoadResourceImage(e.Src);
                }

                if (imgObj != null)
                {
                    e.Callback(imgObj);
                }
                else
                {
                    // If we couldn't load the image, use a default or error image
                    if (pdfSharp)
                    {
                        // Create a simple 1x1 pixel image for PDFs when original can't be loaded
                        using (var blankBmp = new Bitmap(1, 1))
                        using (var g = Graphics.FromImage(blankBmp))
                        {
                            g.Clear(Color.Transparent);
                            using (var ms = new MemoryStream())
                            {
                                blankBmp.Save(ms, ImageFormat.Png);
                                ms.Position = 0;
                                e.Callback(XImage.FromStream(ms));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image {e.Src}: {ex.Message}");
                // Don't rethrow - better to have a missing image than a broken renderer
            }
        }
    }
}
