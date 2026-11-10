using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// A parsed <c>linear-gradient()</c> or <c>repeating-linear-gradient()</c>, the stop positions aren't
    /// resolved yet. Interpolates in sRGB, an interpolation color space isn't supported.
    /// </summary>
    internal sealed class ParsedLinearGradient
    {
        public double AngleRad { get; set; }
        public (RColor? Color, Length? Position, bool IsHint)[] Stops { get; set; }
        public bool IsRepeating { get; set; }
    }
}
