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

using Microsoft.Graphics.Canvas;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WinUI.Adapters
{
    /// <summary>
    /// Adapter for Win2D image object for core.
    /// </summary>
    internal sealed class ImageAdapter : RImage
    {
        /// <summary>
        /// the underline Win2D bitmap.
        /// </summary>
        private readonly CanvasBitmap _bitmap;

        /// <summary>
        /// Init.
        /// </summary>
        public ImageAdapter(CanvasBitmap bitmap)
        {
            _bitmap = bitmap;
        }

        /// <summary>
        /// the underline Win2D bitmap.
        /// </summary>
        public CanvasBitmap Bitmap
        {
            get { return _bitmap; }
        }

        // Pixels rather than DIPs, so the image's DPI doesn't change its layout size
        public override double Width
        {
            get { return _bitmap.SizeInPixels.Width; }
        }

        public override double Height
        {
            get { return _bitmap.SizeInPixels.Height; }
        }

        public override void Dispose()
        {
            _bitmap.Dispose();
        }
    }
}
