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

using System.Windows;
using System.Windows.Media;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WPF graphics path object for core.
    /// </summary>
    internal sealed class GraphicsPathAdapter : RGraphicsPath
    {
        /// <summary>
        /// The actual WPF graphics geometry instance.
        /// </summary>
        private readonly StreamGeometry _geometry = new StreamGeometry();

        /// <summary>
        /// The context used in WPF geometry to render path
        /// </summary>
        private readonly StreamGeometryContext _geometryContext;

        public GraphicsPathAdapter()
        {
            _geometryContext = _geometry.Open();
        }

        public override void Start(double x, double y)
        {
            _geometryContext.BeginFigure(new Point(x, y), true, false);
        }

        public override void LineTo(double x, double y)
        {
            _geometryContext.LineTo(new Point(x, y), true, true);
        }

        public override void ArcTo(double x, double y, double size, Corner corner)
        {
            _geometryContext.ArcTo(new Point(x, y), new Size(size, size), 0, false, SweepDirection.Clockwise, true, true);
        }

        /// <summary>
        /// Close the geometry to so no more path adding is allowed and return the instance so it can be rendered.
        /// </summary>
        public StreamGeometry GetClosedGeometry()
        {
            _geometryContext.Close();
            _geometry.Freeze();
            return _geometry;
        }

        public override void Dispose()
        { }
    }
}