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

using SkiaSharp;
using System.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Utilities
{
    /// <summary>
    /// Utilities for converting SkiaSharp entities to HtmlRenderer core entities.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Convert from SkiaSharp point to core point.
        /// </summary>
        public static RPoint Convert(SKPoint p)
        {
            return new RPoint(p.X, p.Y);
        }

        /// <summary>
        /// Convert from SkiaSharp point to core point.
        /// </summary>
        public static SKPoint[] Convert(RPoint[] points)
        {
            SKPoint[] myPoints = new SKPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
                myPoints[i] = Convert(points[i]);
            return myPoints;
        }

        /// <summary>
        /// Convert from core point to SkiaSharp point.
        /// </summary>
        public static SKPoint Convert(RPoint p)
        {
            return new SKPoint((float)p.X, (float)p.Y);
        }

        /// <summary>
        /// Convert from SkiaSharp size to core size.
        /// </summary>
        public static RSize Convert(SKSize s)
        {
            return new RSize(s.Width, s.Height);
        }

        /// <summary>
        /// Convert from core size to SkiaSharp size.
        /// </summary>
        public static SKSize Convert(RSize s)
        {
            return new SKSize((float)s.Width, (float)s.Height);
        }

        /// <summary>
        /// Convert from SkiaSharp rectangle to core rectangle.
        /// </summary>
        public static RRect Convert(SKRect r)
        {
            return new RRect(r.Left, r.Bottom, r.Width, r.Height);
        }

        /// <summary>
        /// Convert from core rectangle to SkiaSharp rectangle.
        /// </summary>
        public static SKRect Convert(RRect r)
        {
            return new SKRect((float)r.Left, (float)r.Bottom, (float)r.Width, (float)r.Height);
        }

        /// <summary>
        /// Convert from core color to SkiaSharp color.
        /// </summary>
        public static SKColor Convert(RColor c)
        {
            return new SKColor(c.R, c.G, c.B, c.A);
        }

        /// <summary>
        /// Convert from  color to SkiaSharp color.
        /// </summary>
        public static RColor Convert(Color c)
        {
            return RColor.FromArgb(c.A, c.R, c.G, c.B);
        }

    }
}