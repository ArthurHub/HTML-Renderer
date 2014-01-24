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

namespace HtmlRenderer.Core.Interfaces
{
    /// <summary>
    /// atodo: add doc
    /// </summary>
    public interface IGraphicsPath : IDisposable
    {
        /// <summary>
        /// Appends an elliptical arc to the current figure.
        /// </summary>
        void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle);

        /// <summary>
        /// Appends a line segment to this GraphicsPath.
        /// </summary>
        void AddLine(float x1, float y1, float x2, float y2);

        /// <summary>
        /// Closes the current figure and starts a new figure. If the current figure contains a sequence of connected 
        /// lines and curves, the method closes the loop by connecting a line from the endpoint to the starting point.
        /// </summary>
        void CloseFigure();
    }
}
