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

using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using SkiaSharp;
using System.Security.Cryptography.X509Certificates;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Adapter for a Pen.  Skia contains pen information in the SKPaint info, so this also wraps
    /// an SKPaint.
    /// </summary>
    internal sealed class PenAdapter : RPen
    {
        /// <summary>
        /// The skia equiv of a 'pen'.
        /// </summary>
        private readonly SKPaint _pen;

        /// <summary>
        /// Init.
        /// </summary>
        public PenAdapter(SKPaint pen)
        {
            _pen = pen;
        }

        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public SKPaint Pen
        {
            get { return _pen; }
        }

        public override double Width
        {
            get { return _pen.StrokeWidth; }
            set { _pen.StrokeWidth = (float)value; }
        }

        public override RDashStyle DashStyle
        {
            set
            {
                switch (value)
                {
                    case RDashStyle.Solid:
                        _pen.PathEffect = null;
                        //_pen.DashStyle = XDashStyle.Solid;
                        break;
                    case RDashStyle.Dash:
                        _pen.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 10 }, 0);
                        if (Width < 2)
                        {
                            _pen.PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0);
                        }
                        //_pen.DashStyle = XDashStyle.Dash;
                        //if (Width < 2)
                        //    _pen.DashPattern = new[] { 4, 4d }; // better looking
                        break;
                    case RDashStyle.Dot:
                        _pen.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 10 }, 5);
                        //_pen.DashStyle = XDashStyle.Dot;
                        break;

                    //TODO: support these.
                    //case RDashStyle.DashDot:
                    //    _pen.DashStyle = XDashStyle.DashDot;
                    //    break;
                    //case RDashStyle.DashDotDot:
                    //    _pen.DashStyle = XDashStyle.DashDotDot;
                    //    break;
                    //case RDashStyle.Custom:
                    //    _pen.DashStyle = XDashStyle.Custom;
                    //    break;
                    default:
                        _pen.PathEffect = null;
                        //_pen.DashStyle = XDashStyle.Solid
                        break;
                }
            }
        }
    }
}