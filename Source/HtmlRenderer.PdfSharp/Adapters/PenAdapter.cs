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

using HtmlRenderer.Entities;
using HtmlRenderer.Interfaces;
using PdfSharp.Drawing;

namespace HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for WinForms pens objects for core.
    /// </summary>
    internal sealed class PenAdapter : IPen
    {
        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        private readonly XPen _pen;

        /// <summary>
        /// Init.
        /// </summary>
        public PenAdapter(XPen pen)
        {
            _pen = pen;
        }

        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public XPen Pen
        {
            get { return _pen; }
        }

        /// <summary>
        /// Gets or sets the width of this Pen, in units of the Graphics object used for drawing.
        /// </summary>
        public float Width
        {
            get { return (float)_pen.Width; }
            set { _pen.Width = value; }
        }

        /// <summary>
        /// Gets or sets the style used for dashed lines drawn with this Pen.
        /// </summary>
        public DashStyleInt DashStyle
        {
            set
            {
                switch( value )
                {
                    case DashStyleInt.Solid:
                        _pen.DashStyle = XDashStyle.Solid;
                        break;
                    case DashStyleInt.Dash:
                        _pen.DashStyle = XDashStyle.Dash;
                        break;
                    case DashStyleInt.Dot:
                        _pen.DashStyle = XDashStyle.Dot;
                        break;
                    case DashStyleInt.DashDot:
                        _pen.DashStyle = XDashStyle.DashDot;
                        break;
                    case DashStyleInt.DashDotDot:
                        _pen.DashStyle = XDashStyle.DashDotDot;
                        break;
                    case DashStyleInt.Custom:
                        _pen.DashStyle = XDashStyle.Custom;
                        break;
                    default:
                        _pen.DashStyle = XDashStyle.Solid;
                        break;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets an array of custom dashes and spaces.
        /// </summary>
        public float[] DashPattern
        {
            set
            {
                var dValues = new double[value.Length];
                for(int i = 0; i < value.Length; i++)
                    dValues[i] = value[i];
                
                _pen.DashPattern = dValues;
            }
        }
    }
}
