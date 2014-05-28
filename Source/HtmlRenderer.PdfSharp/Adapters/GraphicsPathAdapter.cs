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

using HtmlRenderer.Adapters;
using HtmlRenderer.Adapters.Entities;
using PdfSharp.Drawing;

namespace HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for WinForms graphics path object for core.
    /// </summary>
    internal sealed class GraphicsPathAdapter : RGraphicsPath
    {
        /// <summary>
        /// The actual PdfSharp graphics path instance.
        /// </summary>
        private readonly XGraphicsPath _graphicsPath = new XGraphicsPath();

        /// <summary>
        /// the last point added to the path to begin next segment from
        /// </summary>
        private RPoint _lastPoint;

        /// <summary>
        /// The actual PdfSharp graphics path instance.
        /// </summary>
        public XGraphicsPath GraphicsPath
        {
            get { return _graphicsPath; }
        }

        public override void Start(double x, double y)
        {
            _lastPoint = new RPoint(x, y);
        }

        public override void LineTo(double x, double y)
        {
            _graphicsPath.AddLine((float)_lastPoint.X, (float)_lastPoint.Y, (float)x, (float)y);
            _lastPoint = new RPoint(x, y);
        }

        public override void ArcTo(double x, double y, double size, int startAngle, int sweepAngle)
        {
            float left = (float)(System.Math.Min(x, _lastPoint.X) - (startAngle == 270 || startAngle == 0 ? size : 0));
            float top = (float)(System.Math.Min(y, _lastPoint.Y) - (startAngle == 90 || startAngle == 0 ? size : 0));
            _graphicsPath.AddArc(left, top, (float)size * 2, (float)size * 2, startAngle, sweepAngle);
            _lastPoint = new RPoint(x, y);
        }

        public override void Dispose()
        { }
    }
}