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

using HtmlRenderer.Interfaces;
using PdfSharp.Drawing;

namespace HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for WinForms graphics path object for core.
    /// </summary>
    internal sealed class GraphicsPathAdapter : IGraphicsPath
    {
        /// <summary>
        /// The actual WinForms graphics path instance.
        /// </summary>
        private readonly XGraphicsPath _graphicsPath = new XGraphicsPath();

        /// <summary>
        /// The actual WinForms graphics path instance.
        /// </summary>
        public XGraphicsPath GraphicsPath
        {
            get { return _graphicsPath; }
        }

        /// <summary>
        /// Appends an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            _graphicsPath.AddArc(x, y, width, height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Appends a line segment to this GraphicsPath.
        /// </summary>
        public void AddLine(float x1, float y1, float x2, float y2)
        {
            _graphicsPath.AddLine(x1, y1, x2, y2);
        }

        /// <summary>
        /// Closes the current figure and starts a new figure. If the current figure contains a sequence of connected 
        /// lines and curves, the method closes the loop by connecting a line from the endpoint to the starting point.
        /// </summary>
        public void CloseFigure()
        {
            _graphicsPath.CloseFigure();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
