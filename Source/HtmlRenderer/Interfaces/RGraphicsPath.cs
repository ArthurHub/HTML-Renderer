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

namespace HtmlRenderer.Interfaces
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public abstract class RGraphicsPath : IDisposable
    {
        /// <summary>
        ///  Appends an elliptical arc to the current figure.
        ///  </summary>
        public abstract void AddArc(double x, double y, double width, double height, double startAngle, double sweepAngle);

        /// <summary>
        ///  Appends a line segment to this GraphicsPath.
        ///  </summary>
        public abstract void AddLine(double x1, double y1, double x2, double y2);

        /// <summary>
        ///  Closes the current figure and starts a new figure. If the current figure contains a sequence of connected 
        ///  lines and curves, the method closes the loop by connecting a line from the endpoint to the starting point.
        ///  </summary>
        public abstract void CloseFigure();

        public abstract void Dispose();
    }
}