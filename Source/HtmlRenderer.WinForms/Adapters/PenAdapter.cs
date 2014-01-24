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
using HtmlRenderer.Core;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Core.Interfaces;

namespace HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms pens objects for core.
    /// </summary>
    internal sealed class PenAdapter : IPen
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
        public float Width
        {
            get { return _pen.Width; }
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
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                        break;
                    case DashStyleInt.Dash:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        break;
                    case DashStyleInt.Dot:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        break;
                    case DashStyleInt.DashDot:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                        break;
                    case DashStyleInt.DashDotDot:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
                        break;
                    case DashStyleInt.Custom:
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
        public float[] DashPattern
        {
            set { _pen.DashPattern = value; }
        }
    }
}
