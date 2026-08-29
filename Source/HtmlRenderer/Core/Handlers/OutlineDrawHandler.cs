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
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers
{
    /// <summary>
    /// Paints <c>outline</c> (CSS 2.1 §18.1) as a plain rectangular ring around the box's own border-box
    /// edge - unlike <see cref="BordersDrawHandler"/>, an outline has no per-side bevel/corner-joining to
    /// account for (it isn't part of the box model and doesn't affect layout), so this is a single flat
    /// four-rectangle fill rather than a per-side polygon.<br/>
    /// Deliberately a subset of the full property: only <c>solid</c> (and <c>auto</c>, treated the same
    /// as <c>solid</c> per spec) is painted - <c>dotted</c>/<c>dashed</c>/<c>double</c>/<c>groove</c>/
    /// <c>ridge</c>/<c>inset</c>/<c>outset</c> are not (a box with one of those styles simply paints no
    /// outline, same as <c>none</c>). <c>outline-offset</c> is not implemented either (the CSS-OM here has
    /// no <c>outline-offset</c> property at all to read a value from), so the ring always sits flush
    /// against the border-box edge.
    /// </summary>
    internal static class OutlineDrawHandler
    {
        /// <summary>
        /// Paints <paramref name="box"/>'s outline around <paramref name="borderBoxRect"/> (the same
        /// border-box rectangle <see cref="BordersDrawHandler.DrawBoxBorders"/> was just given for this
        /// line) if it has a paintable one.
        /// </summary>
        public static void Draw(RGraphics g, CssBox box, RRect borderBoxRect)
        {
            var width = box.ActualOutlineWidth;
            if (width <= 0 || borderBoxRect.Width <= 0 || borderBoxRect.Height <= 0) return;

            var style = box.OutlineStyle;
            var isSolid = style == CssConstants.Solid || style == CssConstants.Auto;
            if (!isSolid) return;

            var brush = g.GetSolidBrush(box.ActualOutlineColor);

            // Top band (full width, including the corners) ...
            g.DrawRectangle(brush, borderBoxRect.X - width, borderBoxRect.Y - width, borderBoxRect.Width + 2 * width, width);
            // ... bottom band ...
            g.DrawRectangle(brush, borderBoxRect.X - width, borderBoxRect.Bottom, borderBoxRect.Width + 2 * width, width);
            // ... left band (between the top/bottom bands, not overlapping their corners) ...
            g.DrawRectangle(brush, borderBoxRect.X - width, borderBoxRect.Y, width, borderBoxRect.Height);
            // ... right band.
            g.DrawRectangle(brush, borderBoxRect.Right, borderBoxRect.Y, width, borderBoxRect.Height);
        }
    }
}
