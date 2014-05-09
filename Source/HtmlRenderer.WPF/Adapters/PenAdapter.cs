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

using System.Windows.Media;
using HtmlRenderer.Adapters;
using HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.WPF.Adapters
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
            get { return _pen.Thickness; }
            set { _pen.Thickness = value; }
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
                        _pen.DashStyle = DashStyles.Solid;
                        break;
                    case RDashStyle.Dash:
                        _pen.DashStyle = DashStyles.Dash;
                        break;
                    case RDashStyle.Dot:
                        _pen.DashStyle = DashStyles.Dot;
                        break;
                    case RDashStyle.DashDot:
                        _pen.DashStyle = DashStyles.DashDot;
                        break;
                    case RDashStyle.DashDotDot:
                        _pen.DashStyle = DashStyles.DashDotDot;
                        break;
                    default:
                        _pen.DashStyle = DashStyles.Solid;
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
                // TODO:a handle custom pattern pen
//                var fValues = new float[value.Length];
//                for (int i = 0; i < value.Length; i++)
//                    fValues[i] = (float)value[i];
//                _pen.DashPattern = fValues;
            }
        }
    }
}