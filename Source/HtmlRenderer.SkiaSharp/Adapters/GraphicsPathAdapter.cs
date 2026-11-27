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
    /// Adapter for SkiaSharp paths.
    /// </summary>
    internal sealed class GraphicsPathAdapter : RGraphicsPath
    {
        private readonly SKPathBuilder _builder = new SKPathBuilder();

        /// <summary>
        /// The path, built on first use.
        /// </summary>
        private SKPath _path;

        public override void Start(double x, double y)
        {
            _builder.MoveTo((float)x, (float)y);
        }

        public override void LineTo(double x, double y)
        {
            _builder.LineTo((float)x, (float)y);
        }

        public override void ArcTo(double x, double y, double radiusX, double radiusY, Corner corner)
        {
            // The core walks the shapes clockwise, so each corner is the small clockwise arc to the end point
            _builder.ArcTo((float)radiusX, (float)radiusY, 0, SKPathArcSize.Small, SKPathDirection.Clockwise, (float)x, (float)y);
        }

        /// <summary>
        /// The path to draw or fill.
        /// </summary>
        /// <remarks>
        /// Left open, the core strokes each rounded border side as its own open path, filling closes it anyway.
        /// </remarks>
        public SKPath GetPath()
        {
            return _path ?? (_path = _builder.Detach());
        }

        public override void Dispose()
        {
            _path?.Dispose();
            _builder.Dispose();
        }
    }
}
