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
using System.Drawing.Drawing2D;
using HtmlRenderer.Adapters;
using HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms graphics path object for core.
    /// </summary>
    internal sealed class GraphicsPathAdapter : RGraphicsPath
    {
        /// <summary>
        /// The actual WinForms graphics path instance.
        /// </summary>
        private readonly GraphicsPath _graphicsPath = new GraphicsPath();

        /// <summary>
        /// the last point added to the path to begin next segment from
        /// </summary>
        private RPoint _lastPoint;

        /// <summary>
        /// The actual WinForms graphics path instance.
        /// </summary>
        public GraphicsPath GraphicsPath
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
            float left = (float)(Math.Min(x, _lastPoint.X) - (startAngle == 270 || startAngle == 0 ? size : 0));
            float top = (float)(Math.Min(y, _lastPoint.Y) - (startAngle == 90 || startAngle == 0 ? size : 0));
            _graphicsPath.AddArc(left, top, (float)size*2, (float)size*2, startAngle, sweepAngle);
            _lastPoint = new RPoint(x, y);
        }

        /// <summary>
        /// Appends an elliptical arc to the current figure.
        /// </summary>
        public override void AddArc(double x, double y, double width, double height, int startAngle, int sweepAngle)
        {
            _graphicsPath.AddArc((float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);
        }

        /// <summary>
        /// Appends a line segment to this GraphicsPath.
        /// </summary>
        public override void AddLine(double x1, double y1, double x2, double y2)
        {
            _graphicsPath.AddLine((float)x1, (float)y1, (float)x2, (float)y2);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _graphicsPath.Dispose();
        }
    }
}