﻿// "Therefore those skilled at the unorthodox
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
using PdfSharp.Drawing;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for WinForms pens objects for core.
    /// </summary>
    internal sealed class PenAdapter : RPen
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

        public override double Width
        {
            get { return _pen.Width; }
            set { _pen.Width = value; }
        }

        public override RDashStyle DashStyle
        {
            set
            {
                switch (value)
                {
                    case RDashStyle.Solid:
                        _pen.DashStyle = XDashStyle.Solid;
                        break;
                    case RDashStyle.Dash:
                        _pen.DashStyle = XDashStyle.Dash;
                        if (Width < 2)
                            _pen.DashPattern = new[] { 4, 4d }; // better looking
                        break;
                    case RDashStyle.Dot:
                        _pen.DashStyle = XDashStyle.Dot;
                        break;
                    case RDashStyle.DashDot:
                        _pen.DashStyle = XDashStyle.DashDot;
                        break;
                    case RDashStyle.DashDotDot:
                        _pen.DashStyle = XDashStyle.DashDotDot;
                        break;
                    case RDashStyle.Custom:
                        _pen.DashStyle = XDashStyle.Custom;
                        break;
                    default:
                        _pen.DashStyle = XDashStyle.Solid;
                        break;
                }
            }
        }
    }
}