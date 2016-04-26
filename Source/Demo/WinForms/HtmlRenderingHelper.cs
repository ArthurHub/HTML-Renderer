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
            Image image;
            if (!_imageCache.TryGetValue(src, out image))
            {
                var imageStream = DemoUtils.GetImageStream(src);
                if (imageStream != null)
                {
                    image = Image.FromStream(imageStream);
                    _imageCache[src] = image;
                }
            }
            return image;
        }

        /// <summary>
        /// Get image by resource key.
        /// </summary>
        public static XImage TryLoadResourceXImage(string src)
        {
            var img = TryLoadResourceImage(src);
            XImage xImg;

            if (img == null)
                return null;

            using (var ms = new MemoryStream())
            {
                img.Save(ms, img.RawFormat);
                xImg = img != null ? XImage.FromStream(ms) : null;
            }

            return xImg;
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
            var img = TryLoadResourceImage(e.Src);
            XImage xImg = null;

            if (img != null)
            {
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, img.RawFormat);
                    xImg = img != null ? XImage.FromStream(ms) : null;
                }
            }

            object imgObj;
            if (pdfSharp)
                imgObj = xImg;
            else
                imgObj = img;

            if (!e.Handled && e.Attributes != null)
            {
                if (e.Attributes.ContainsKey("byevent"))
                {
                    int delay;
                    if (Int32.TryParse(e.Attributes["byevent"], out delay))
                    {
                        e.Handled = true;
                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            Thread.Sleep(delay);
                            e.Callback("https://fbcdn-sphotos-a-a.akamaihd.net/hphotos-ak-snc7/c0.44.403.403/p403x403/318890_10151195988833836_1081776452_n.jpg");
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
                    var split = e.Attributes["byrect"].Split(',');
                    var rect = new Rectangle(Int32.Parse(split[0]), Int32.Parse(split[1]), Int32.Parse(split[2]), Int32.Parse(split[3]));
                    e.Callback(imgObj ?? TryLoadResourceImage("htmlicon"), rect.X, rect.Y, rect.Width, rect.Height);
                    return;
                }
            }

            if (img != null)
                e.Callback(imgObj);
        }
    }
}