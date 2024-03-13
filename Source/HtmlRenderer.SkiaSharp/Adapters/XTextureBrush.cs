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

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Because PdfSharp doesn't support texture brush we need to implement it ourselves.
    /// </summary>
    internal sealed class XTextureBrush
    {
        #region Fields/Consts

        /// <summary>
        /// The image to draw in the brush
        /// </summary>
        private readonly SKImage _image;

        /// <summary>
        /// the
        /// </summary>
        private readonly SKRect _dstRect;

        /// <summary>
        /// the transform the location of the image to handle center alignment
        /// </summary>
        private readonly SKPoint _translateTransformLocation;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public XTextureBrush(SKImage image, SKRect dstRect, SKPoint translateTransformLocation)
        {
            _image = image;
            _dstRect = dstRect;
            _translateTransformLocation = translateTransformLocation;
        }

        /// <summary>
        /// Draw the texture image in the given graphics at the given location.
        /// </summary>
        public void DrawRectangle(SKCanvas g, float x, float y, float width, float height)
        {
            var prevState = g.Save();
            g.ClipRect(new SKRect(x, y, width, height));

            float rx = _translateTransformLocation.X;
            float w = _image.Width, h = _image.Height;
            while (rx < x + width)
            {
                float ry = _translateTransformLocation.Y;
                while (ry < y + height)
                {
                    g.DrawImage(_image, new SKRect(rx, ry, w, h));
                    ry += h;
                }
                rx += w;
            }

            g.Restore();
        }
    }
}