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
        /// The actual WPF brush instance.
        /// </summary>
        private readonly Brush _brush;

        /// <summary>
        /// the width of the pen
        /// </summary>
        private double _width;

        /// <summary>
        /// the dash style of the pen
        /// </summary>
        private DashStyle _dashStyle = DashStyles.Solid;

        /// <summary>
        /// Init.
        /// </summary>
        public PenAdapter(Brush brush)
        {
            _brush = brush;
        }

        /// <summary>
        /// Gets or sets the width of this Pen, in units of the Graphics object used for drawing.
        /// </summary>
        public override double Width
        {
            get { return _width; }
            set { _width = value; }
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
                        _dashStyle = DashStyles.Solid;
                        break;
                    case RDashStyle.Dash:
                        _dashStyle = DashStyles.Dash;
                        break;
                    case RDashStyle.Dot:
                        _dashStyle = DashStyles.Dot;
                        break;
                    case RDashStyle.DashDot:
                        _dashStyle = DashStyles.DashDot;
                        break;
                    case RDashStyle.DashDotDot:
                        _dashStyle = DashStyles.DashDotDot;
                        break;
                    default:
                        _dashStyle = DashStyles.Solid;
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

        /// <summary>
        /// Create the actual WPF pen instance.
        /// </summary>
        public Pen CreatePen()
        {
            var pen = new Pen(_brush, _width);
            pen.DashStyle = _dashStyle;
            return pen;
        }
    }
}