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

using PdfSharp.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.Utilities
{
    /// <summary>
    /// Utilities for converting WinForms entities to HtmlRenderer core entities.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Convert from WinForms point to core point.
        /// </summary>
        public static RPoint Convert(XPoint p)
        {
            return new RPoint(p.X, p.Y);
        }

        /// <summary>
        /// Convert from WinForms point to core point.
        /// </summary>
        public static XPoint[] Convert(RPoint[] points)
        {
            XPoint[] myPoints = new XPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
                myPoints[i] = Convert(points[i]);
            return myPoints;
        }

        /// <summary>
        /// Convert from core point to WinForms point.
        /// </summary>
        public static XPoint Convert(RPoint p)
        {
            return new XPoint(p.X, p.Y);
        }

        /// <summary>
        /// Convert from WinForms size to core size.
        /// </summary>
        public static RSize Convert(XSize s)
        {
            return new RSize(s.Width, s.Height);
        }

        /// <summary>
        /// Convert from core size to WinForms size.
        /// </summary>
        public static XSize Convert(RSize s)
        {
            return new XSize(s.Width, s.Height);
        }

        /// <summary>
        /// Convert from WinForms rectangle to core rectangle.
        /// </summary>
        public static RRect Convert(XRect r)
        {
            return new RRect(r.X, r.Y, r.Width, r.Height);
        }

        /// <summary>
        /// Convert from core rectangle to WinForms rectangle.
        /// </summary>
        public static XRect Convert(RRect r)
        {
            return new XRect(r.X, r.Y, r.Width, r.Height);
        }

        /// <summary>
        /// Convert from core color to WinForms color.
        /// </summary>
        public static XColor Convert(RColor c)
        {
            return XColor.FromArgb(c.A, c.R, c.G, c.B);
        }

        /// <summary>
        /// Convert from PDFsharp color to WinForms color.
        /// </summary>
        public static RColor Convert(XColor c)
        {
            return RColor.FromArgb((byte)(c.A * 255.0), c.R, c.G, c.B);
        }

        /// <summary>
        /// Convert from core font style to PDFsharp font style.
        /// </summary>
        public static XFontStyleEx Convert(RFontStyle style)
        {
            var fontStyle = XFontStyleEx.Regular;

            if ((style & RFontStyle.Bold) == RFontStyle.Bold)
                fontStyle |= XFontStyleEx.Bold;

            if ((style & RFontStyle.Italic) == RFontStyle.Italic)
                fontStyle |= XFontStyleEx.Italic;

            if ((style & RFontStyle.Underline) == RFontStyle.Underline)
                fontStyle |= XFontStyleEx.Underline;

            if ((style & RFontStyle.Strikeout) == RFontStyle.Strikeout)
                fontStyle |= XFontStyleEx.Strikeout;

            return fontStyle;
        }

    }
}