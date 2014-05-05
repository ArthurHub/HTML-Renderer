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
using HtmlRenderer.Entities;
using HtmlRenderer.Interfaces;

namespace HtmlRenderer.WinForms.Adapters
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

        /// <summary>
        /// Gets or sets the width of this Pen, in units of the Graphics object used for drawing.
        /// </summary>
        public override double Width
        {
            get { return _pen.Width; }
            set { _pen.Width = (float)value; }
        }

        /// <summary>
        /// Gets or sets the style used for dashed lines drawn with this Pen.
        /// </summary>
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

        /// <summary>
        /// Gets or sets an array of custom dashes and spaces.
        /// </summary>
        public override double[] DashPattern
        {
            set
            {
                var fValues = new float[value.Length];
                for (int i = 0; i < value.Length; i++)
                    fValues[i] = (float)value[i];
                _pen.DashPattern = fValues;
            }
        }
    }
}