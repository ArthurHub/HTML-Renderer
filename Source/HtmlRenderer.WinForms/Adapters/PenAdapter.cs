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

using System.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms pens objects for core.
    /// </summary>
    internal sealed class PenAdapter : RPen
    {
        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        private readonly Pen _pen;

        /// <summary>
        /// Init.
        /// </summary>
        public PenAdapter(Pen pen)
        {
            _pen = pen;
        }

        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public Pen Pen
        {
            get { return _pen; }
        }

        public override double Width
        {
            get { return _pen.Width; }
            set { _pen.Width = (float)value; }
        }

        public override RDashStyle DashStyle
        {
            set
            {
                switch (value)
                {
                    case RDashStyle.Solid:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                        break;
                    case RDashStyle.Dash:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        if (Width < 2)
                            _pen.DashPattern = new[] { 4, 4f }; // better looking
                        break;
                    case RDashStyle.Dot:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        break;
                    case RDashStyle.DashDot:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                        break;
                    case RDashStyle.DashDotDot:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
                        break;
                    case RDashStyle.Custom:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                        break;
                    default:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                        break;
                }
            }
        }
    }
}