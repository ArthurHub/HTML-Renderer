using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    [Flags]
    internal enum PropertyFlags : byte
    {
        None = 0,
        Inherited = 1,
        Hashless = 2,
        Unitless = 4,
        Animatable = 8,
        Shorthand = 16
    }
}