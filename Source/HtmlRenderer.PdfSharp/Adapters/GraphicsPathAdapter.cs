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
using PdfSharp.Drawing;

namespace HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for WinForms graphics path object for core.
    /// </summary>
    internal sealed class GraphicsPathAdapter : RGraphicsPath
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

        public override void AddArc(double x, double y, double width, double height, double startAngle, double sweepAngle)
        {
            _graphicsPath.AddArc(x, y, width, height, startAngle, sweepAngle);
        }

        public override void AddLine(double x1, double y1, double x2, double y2)
        {
            _graphicsPath.AddLine(x1, y1, x2, y2);
        }

        public override void CloseFigure()
        {
            _graphicsPath.CloseFigure();
        }

        public override void Dispose()
        { }
    }
}