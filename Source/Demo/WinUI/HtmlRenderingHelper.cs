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
using System.IO;
using System.Threading;
using Microsoft.Graphics.Canvas;
using PdfSharp.Drawing;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using Windows.Storage.Streams;

namespace TheArtOfDev.HtmlRenderer.Demo.WinUI
{
    internal static class HtmlRenderingHelper
    {
        private const string ByEventImageUrl = "https://images.unsplash.com/photo-1608848461950-0fe51dfc41cb?w=500&q=80";

        /// <summary>
        /// Cache for resource images, the images are loaded from the render threads as well
        /// </summary>
        private static readonly Dictionary<string, CanvasBitmap> _imageCache = new Dictionary<string, CanvasBitmap>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Get image by resource key.
        /// </summary>
        public static CanvasBitmap TryLoadResourceImage(string src)
        {
            lock (_imageCache)
            {
                if (!_imageCache.TryGetValue(src, out var image))
                {
                    using var stream = DemoUtils.GetImageStream(src);
                    if (stream != null)
                    {
                        image = CanvasBitmap.LoadAsync(CanvasDevice.GetSharedDevice(), stream.AsRandomAccessStream()).AsTask().GetAwaiter().GetResult();
                        _imageCache[src] = image;
                    }
                }
                return image;
            }
        }

        /// <summary>
        /// Get the resource image of the given key as data URI, to show the html in a browser.
        /// </summary>
        public static string TryGetResourceImageDataUri(string src)
        {
            using var stream = DemoUtils.GetImageStream(src);
            if (stream == null)
                return null;

            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            var bytes = memory.ToArray();
            var mimeType = bytes.Length > 3 && bytes[0] == 'G' && bytes[1] == 'I' && bytes[2] == 'F' ? "image/gif" : "image/png";
            return "data:" + mimeType + ";base64," + Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// On image load in renderer set the image by event async.
        /// </summary>
        public static void OnImageLoad(object sender, HtmlImageLoadEventArgs args)
        {
            ImageLoad(args, false);
        }

        /// <summary>
        /// On image load in renderer set the image by event async.
        /// </summary>
        public static void OnImageLoadPdfSharp(object sender, HtmlImageLoadEventArgs args)
        {
            ImageLoad(args, true);
        }

        /// <summary>
        /// On image load in renderer set the image by event async.
        /// </summary>
        private static void ImageLoad(HtmlImageLoadEventArgs e, bool pdfSharp)
        {
            var img = TryLoadResourceImage(e.Src);
            var imgObj = pdfSharp ? (object)(img != null ? CreatePdfSharpImage(img) : null) : img;

            if (!e.Handled && e.Attributes != null)
            {
                if (e.Attributes.ContainsKey("byevent"))
                {
                    if (int.TryParse(e.Attributes["byevent"], out var delay))
                    {
                        e.Handled = true;
                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            Thread.Sleep(delay);
                            e.Callback(ByEventImageUrl);
                        });
                    }
                    else
                    {
                        e.Callback(ByEventImageUrl);
                    }
                    return;
                }

                if (e.Attributes.ContainsKey("byrect"))
                {
                    var split = e.Attributes["byrect"].Split(',');
                    var defaultResourceImage = TryLoadResourceImage("htmlicon");
                    var defaultImg = pdfSharp && defaultResourceImage != null ? (object)CreatePdfSharpImage(defaultResourceImage) : defaultResourceImage;
                    e.Callback(imgObj ?? defaultImg, int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3]));
                    return;
                }
            }

            if (img != null)
                e.Callback(imgObj);
        }

        /// <summary>
        /// Convert the image to PNG before creating the <see cref="XImage"/>, PdfSharp doesn't support GIF.
        /// </summary>
        private static XImage CreatePdfSharpImage(CanvasBitmap image)
        {
            using var stream = new InMemoryRandomAccessStream();
            image.SaveAsync(stream, CanvasBitmapFileFormat.Png).AsTask().GetAwaiter().GetResult();

            stream.Seek(0);
            var memory = new MemoryStream();
            stream.AsStreamForRead().CopyTo(memory);
            memory.Position = 0;
            return XImage.FromStream(memory);
        }
    }
}
