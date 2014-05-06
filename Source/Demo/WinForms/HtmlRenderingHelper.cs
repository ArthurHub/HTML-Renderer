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
using System.Drawing;
using System.Threading;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Demo.Common;
using PdfSharp.Drawing;

namespace HtmlRenderer.Demo.WinForms
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
        /// Get stylesheet by given key.
        /// </summary>
        public static string GetStylesheet(string src)
        {
            if (src == "StyleSheet")
            {
                return @"h1, h2, h3 { color: navy; font-weight:normal; }
                    h1 { margin-bottom: .47em }
                    h2 { margin-bottom: .3em }
                    h3 { margin-bottom: .4em }
                    ul { margin-top: .5em }
                    ul li {margin: .25em}
                    body { font:10pt Tahoma }
		            pre  { border:solid 1px gray; background-color:#eee; padding:1em }
                    a:link { text-decoration: none; }
                    a:hover { text-decoration: underline; }
                    .gray    { color:gray; }
                    .example { background-color:#efefef; corner-radius:5px; padding:0.5em; }
                    .whitehole { background-color:white; corner-radius:10px; padding:15px; }
                    .caption { font-size: 1.1em }
                    .comment { color: green; margin-bottom: 5px; margin-left: 3px; }
                    .comment2 { color: green; }";
            }
            return null;
        }

        /// <summary>
        /// Get image by resource key.
        /// </summary>
        public static Image TryLoadResourceImage(string src)
        {
            Image image;
            if (!_imageCache.TryGetValue(src, out image))
            {
                switch (src.ToLower())
                {
                    case "htmlicon":
                        image = Image.FromStream(Resources.Html32);
                        break;
                    case "staricon":
                        image = Image.FromStream(Resources.Favorites32);
                        break;
                    case "fonticon":
                        image = Image.FromStream(Resources.Font32);
                        break;
                    case "commenticon":
                        image = Image.FromStream(Resources.Comment16);
                        break;
                    case "imageicon":
                        image = Image.FromStream(Resources.Image32);
                        break;
                    case "methodicon":
                        image = Image.FromStream(Resources.Method16);
                        break;
                    case "propertyicon":
                        image = Image.FromStream(Resources.Property16);
                        break;
                    case "eventicon":
                        image = Image.FromStream(Resources.Event16);
                        break;
                }

                if (image != null)
                    _imageCache[src] = image;
            }
            return image;
        }

        /// <summary>
        /// Get image by resource key.
        /// </summary>
        public static XImage TryLoadResourceXImage(string src)
        {
            var img = TryLoadResourceImage(src);
            return img != null ? XImage.FromGdiPlusImage(img) : null;
        }

        /// <summary>
        /// Handle stylesheet resolve.
        /// </summary>
        public static void OnStylesheetLoad(object sender, HtmlStylesheetLoadEventArgs e)
        {
            var stylesheet = GetStylesheet(e.Src);
            if (stylesheet != null)
                e.SetStyleSheet = stylesheet;
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
            var xImg = img != null ? XImage.FromGdiPlusImage(img) : null;
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