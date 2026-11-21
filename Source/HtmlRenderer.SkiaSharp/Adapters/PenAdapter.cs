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
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Adapter for SkiaSharp strokes.
    /// </summary>
    /// <remarks>
    /// The core changes width and dash style of the cached pens before every use, so the paint is set up per draw call.
    /// </remarks>
    internal sealed class PenAdapter : RPen
    {
        private readonly SKColor _color;

        private double _width = 1;

        private RDashStyle _dashStyle = RDashStyle.Solid;

        public PenAdapter(SKColor color)
        {
            _color = color;
        }

        public override double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// A width between 0 and 1 is drawn 1 wide, like GDI+ and the border width snapping of CSS Values 4 §6.
        /// Narrower non-antialiased strokes fall between the pixel centers and disappear.
        /// </summary>
        public float StrokeWidth
        {
            get { return _width > 0 && _width < 1 ? 1f : (float)_width; }
        }

        public override RDashStyle DashStyle
        {
            set { _dashStyle = value; }
        }

        /// <summary>
        /// Sets up the paint to stroke with this pen, the returned path effect has to be disposed after drawing.
        /// </summary>
        public SKPathEffect Apply(SKPaint paint)
        {
            var width = StrokeWidth;
            paint.Style = SKPaintStyle.Stroke;
            paint.Color = _color;
            paint.Shader = null;
            paint.StrokeWidth = width;

            // The same patterns as GDI+, in multiples of the width
            float[] intervals;
            switch (_dashStyle)
            {
                case RDashStyle.Dash:
                    intervals = new[] { 3 * width, width };
                    break;
                case RDashStyle.Dot:
                    intervals = new[] { width, width };
                    break;
                case RDashStyle.DashDot:
                    intervals = new[] { 3 * width, width, width, width };
                    break;
                case RDashStyle.DashDotDot:
                    intervals = new[] { 3 * width, width, width, width, width, width };
                    break;
                default:
                    intervals = null;
                    break;
            }

            var effect = intervals != null && width > 0 ? SKPathEffect.CreateDash(intervals, 0) : null;
            paint.PathEffect = effect;
            return effect;
        }
    }
}
