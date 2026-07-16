using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IGradient : IImageSource
    {
        IEnumerable<GradientStop> Stops { get; }
        bool IsRepeating { get; }
    }
}