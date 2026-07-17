using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// A fully parsed <c>linear-gradient()</c>/<c>repeating-linear-gradient()</c> value - the angle and
    /// raw (not yet position-resolved) color stop list. Ported from PeachPDF's
    /// Html/Core/Entities/ParsedLinearGradient.cs, minus CSS Color 4 interpolation-color-space support
    /// (ColorSpace/HueMethod) - gradients always interpolate in sRGB here.
    /// </summary>
    internal sealed class ParsedLinearGradient
    {
        public double AngleRad { get; set; }
        public (RColor? Color, Length? Position, bool IsHint)[] Stops { get; set; }
        public bool IsRepeating { get; set; }
    }
}
