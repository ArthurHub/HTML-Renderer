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

using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.WinUI.Adapters
{
    /// <summary>
    /// Adapter for Win2D pen (stroke) objects for core.
    /// </summary>
    /// <remarks>
    /// Win2D has no pen, brush, width and stroke style are passed to every draw call.
    /// </remarks>
    internal sealed class PenAdapter : RPen
    {
        /// <summary>
        /// The actual Win2D brush instance.
        /// </summary>
        private readonly ICanvasBrush _brush;

        /// <summary>
        /// the width of the pen
        /// </summary>
        private double _width;

        /// <summary>
        /// the dash/stroke style of the pen
        /// </summary>
        private CanvasStrokeStyle _strokeStyle = new CanvasStrokeStyle();

        /// <summary>
        /// Init.
        /// </summary>
        public PenAdapter(ICanvasBrush brush)
        {
            _brush = brush;
        }

        public override double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public override RDashStyle DashStyle
        {
            set
            {
                var style = new CanvasStrokeStyle();
                switch (value)
                {
                    case RDashStyle.Solid:
                        style.DashStyle = CanvasDashStyle.Solid;
                        break;
                    case RDashStyle.Dash:
                        style.DashStyle = CanvasDashStyle.Dash;
                        break;
                    case RDashStyle.Dot:
                        style.DashStyle = CanvasDashStyle.Dot;
                        break;
                    case RDashStyle.DashDot:
                        style.DashStyle = CanvasDashStyle.DashDot;
                        break;
                    case RDashStyle.DashDotDot:
                        style.DashStyle = CanvasDashStyle.DashDotDot;
                        break;
                    default:
                        style.DashStyle = CanvasDashStyle.Solid;
                        break;
                }
                _strokeStyle = style;
            }
        }

        /// <summary>
        /// The actual Win2D brush instance to stroke with.
        /// </summary>
        public ICanvasBrush Brush
        {
            get { return _brush; }
        }

        /// <summary>
        /// The stroke style (dash pattern) to stroke with.
        /// </summary>
        public CanvasStrokeStyle StrokeStyle
        {
            get { return _strokeStyle; }
        }
    }
}
