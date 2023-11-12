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

using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters;
using static System.Net.Mime.MediaTypeNames;
using TheArtOfDev.HtmlRenderer.SkiaSharp.Utilities;


namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Adapter for SkiaSharp Image object for core.
    /// </summary>
    internal sealed class ImageAdapter : RImage
    {
        /// <summary>
        /// the underline win-forms image.
        /// </summary>
        private readonly SKImage _image;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ImageAdapter(SKImage image)
        {
            _image = image;
        }

        /// <summary>
        /// the underline SkiaSharp image.
        /// </summary>
        public SKImage Image
        {
            get { return _image; }
        }

        /// <summary>
        /// Picture if this represents a structured image, eg, SVG.  Not implemented yet!!
        /// </summary>
        public SKPicture Picture { get; set; }

        public override double Width
        {
            get { return _image.Width; }
        }

        public override double Height
        {
            get { return _image.Height; }
        }

        public override void Dispose()
        {
            _image.Dispose();
        }

        internal void DrawImage(SKCanvas canvas, SKRect dstRect, SKRect? srcRect = null)
        {

            if (Picture != null)
            {
                //TODO: support the overload that passes a source rect.   Using Matrix overload perhaps?..
                canvas.DrawPicture(Picture, dstRect.Location);
            }
            else
            {
                if (srcRect != null)
                {
                    canvas.DrawImage(Image, dstRect, srcRect.Value);
                }
                else
                {
                    canvas.DrawImage(Image, dstRect);
                }
            }
        }
    }
}