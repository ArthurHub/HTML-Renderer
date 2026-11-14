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

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Adapter for SkiaSharp brushes: a solid color, or a shader for gradients and images.
    /// </summary>
    internal sealed class BrushAdapter : RBrush
    {
        private readonly SKColor _color;

        private SKShader _shader;

        public BrushAdapter(SKColor color)
        {
            _color = color;
        }

        public BrushAdapter(SKShader shader)
        {
            _color = SKColors.Black;
            _shader = shader;
        }

        /// <summary>
        /// Sets up the paint to fill with this brush.
        /// </summary>
        public void Apply(SKPaint paint)
        {
            paint.Style = SKPaintStyle.Fill;
            paint.Color = _color;
            paint.Shader = _shader;
        }

        /// <summary>
        /// Only releases the shader, the core also disposes the cached solid brushes and uses them again afterwards.
        /// </summary>
        public override void Dispose()
        {
            _shader?.Dispose();
            _shader = null;
        }
    }
}
