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
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;


namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Adapter for SkiaSharp Image object for core.
    /// </summary>
    internal sealed class ImageAdapter : RImage
    {
        /// <summary>
        /// the underlying image.  This may be either a bitmap (_image) or an svg (_svg).
        /// </summary>
        private SKImage _image;

        private readonly SKSvg? _svg;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ImageAdapter(SKImage image)
        {
            _image = image;
        }

        public ImageAdapter(SKSvg svg)
        {
            _svg = svg;
        }


        /// <summary>
        /// the underline SkiaSharp image.
        /// </summary>
        public SKImage Image
        {
            get 
            { 
                if (_image == null && _svg != null) 
                {
                    //Render the image from the picture, as this is being used in a texture brush.
                    _image = SKImage.FromBitmap(new SKBitmap((int)_svg.CanvasSize.Width, (int)_svg.CanvasSize.Height));
                }
                return _image;
            }
        }

        /// <summary>
        /// Picture if this represents a structured image, eg, SVG.
        /// </summary>
        public SKPicture Picture { get; set; }

        public override double Width
        {
            get { return _image?.Width ?? ((int?)_svg?.CanvasSize.Width) ?? 0; }
        }

        public override double Height
        {
            get { return _image?.Height ?? ((int?)_svg?.CanvasSize.Height) ?? 0; }
        }

        public override void Dispose()
        {
            _image?.Dispose();
            _svg?.Picture?.Dispose();
        }

        internal void DrawImage(SKCanvas canvas, SKRect dstRect, SKRect? srcRect = null)
        {

            if (_svg != null)
            {
                //TODO: support the overload that passes a source rect.   Using Matrix overload perhaps?..
                var matrix = SKMatrix.CreateTranslation(dstRect.Left, dstRect.Top);
                matrix.ScaleX = dstRect.Width / _svg.Picture.CullRect.Width;
                matrix.ScaleY = dstRect.Height / _svg.Picture.CullRect.Height;

                canvas.DrawPicture(_svg.Picture, ref matrix);
            }
            else
            {
                if (srcRect != null)
                {
                    canvas.DrawImage(_image, dstRect, srcRect.Value);
                }
                else
                {
                    canvas.DrawImage(_image, dstRect);
                }
            }
        }
    }
}