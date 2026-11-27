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
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Utilities
{
    /// <summary>
    /// Conversions between the core and the SkiaSharp types.
    /// </summary>
    internal static class Utils
    {
        public static RPoint Convert(SKPoint p)
        {
            return new RPoint(p.X, p.Y);
        }

        public static SKPoint Convert(RPoint p)
        {
            return new SKPoint((float)p.X, (float)p.Y);
        }

        public static RSize Convert(SKSize s)
        {
            return new RSize(s.Width, s.Height);
        }

        public static SKSize Convert(RSize s)
        {
            return new SKSize((float)s.Width, (float)s.Height);
        }

        public static SKSizeI ConvertRound(RSize s)
        {
            return new SKSizeI((int)System.Math.Round(s.Width), (int)System.Math.Round(s.Height));
        }

        public static RRect Convert(SKRect r)
        {
            return new RRect(r.Left, r.Top, r.Width, r.Height);
        }

        public static SKRect Convert(RRect r)
        {
            return SKRect.Create((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
        }

        public static RColor Convert(SKColor c)
        {
            return RColor.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
        }

        public static SKColor Convert(RColor c)
        {
            return new SKColor(c.R, c.G, c.B, c.A);
        }
    }
}
