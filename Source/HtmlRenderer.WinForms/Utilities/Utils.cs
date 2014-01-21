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

using System;
using System.Drawing;
using HtmlRenderer.Core.SysEntities;

namespace HtmlRenderer.WinForms.Utilities
{
    /// <summary>
    /// Utilities for converting WinForms entities to HtmlRenderer core entities.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Convert from WinForms point to core point.
        /// </summary>
        public static PointInt Convert(PointF p)
        {
            return new PointInt(p.X, p.Y);
        }

        /// <summary>
        /// Convert from WinForms point to core point.
        /// </summary>
        public static PointF[] Convert(PointInt[] points)
        {
            PointF[] myPoints = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
                myPoints[i] = Convert(points[i]);
            return myPoints;
        }

        /// <summary>
        /// Convert from core point to WinForms point.
        /// </summary>
        public static PointF Convert(PointInt p)
        {
            return new PointF(p.X, p.Y);
        }

        /// <summary>
        /// Convert from core point to WinForms point.
        /// </summary>
        public static Point ConvertRound(PointInt p)
        {
            return new Point((int)Math.Round(p.X), (int)Math.Round(p.Y));
        }

        /// <summary>
        /// Convert from WinForms size to core size.
        /// </summary>
        public static SizeInt Convert(SizeF s)
        {
            return new SizeInt(s.Width, s.Height);
        }

        /// <summary>
        /// Convert from core size to WinForms size.
        /// </summary>
        public static SizeF Convert(SizeInt s)
        {
            return new SizeF(s.Width, s.Height);
        }

        /// <summary>
        /// Convert from core size to WinForms size.
        /// </summary>
        public static Size ConvertRound(SizeInt s)
        {
            return new Size((int)Math.Round(s.Width), (int)Math.Round(s.Height));
        }

        /// <summary>
        /// Convert from WinForms rectangle to core rectangle.
        /// </summary>
        public static RectangleInt Convert(RectangleF r)
        {
            return new RectangleInt(r.X, r.Y, r.Width, r.Height);
        }

        /// <summary>
        /// Convert from core rectangle to WinForms rectangle.
        /// </summary>
        public static RectangleF Convert(RectangleInt r)
        {
            return new RectangleF(r.X, r.Y, r.Width, r.Height);
        }

        /// <summary>
        /// Convert from core rectangle to WinForms rectangle.
        /// </summary>
        public static Rectangle ConvertRound(RectangleInt r)
        {
            return new Rectangle((int)Math.Round(r.X), (int)Math.Round(r.Y), (int)Math.Round(r.Width), (int)Math.Round(r.Height));
        }

        /// <summary>
        /// Convert from WinForms color to core color.
        /// </summary>
        public static ColorInt Convert(Color c)
        {
            return ColorInt.FromArgb(c.A, c.R, c.G, c.B);
        }

        /// <summary>
        /// Convert from core color to WinForms color.
        /// </summary>
        public static Color Convert(ColorInt c)
        {
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }
}
