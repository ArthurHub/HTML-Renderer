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

namespace HtmlRenderer.Adapters
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public abstract class RGraphicsPath : IDisposable
    {
        /// <summary>
        ///  Appends an elliptical arc to the current figure.
        ///  </summary>
        public abstract void AddArc(double x, double y, double size, double height, int startAngle, int sweepAngle);

        /// <summary>
        ///  Appends a line segment to this GraphicsPath.
        ///  </summary>
        public abstract void AddLine(double x1, double y1, double x2, double y2);

        /// <summary>
        /// Start path at the given point.
        /// </summary>
        public abstract void Start(double x, double y);
        
        /// <summary>
        ///  Add stright line to the given point from te last point.
        ///  </summary>
        public abstract void LineTo(double x, double y);
        
        /// <summary>
        /// Add circular arc of the given size to the given point from the last point.
        /// </summary>
        public abstract void ArcTo(double x, double y, double size, int startAngle, int sweepAngle);
        
        /// <summary>
        /// Release path resources.
        /// </summary>
        public abstract void Dispose();
        
    }
}