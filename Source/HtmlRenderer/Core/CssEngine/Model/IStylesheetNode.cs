using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IStylesheetNode : IStyleFormattable
    {
        IEnumerable<IStylesheetNode> Children { get; }
        StylesheetText StylesheetText { get; }
    }
}