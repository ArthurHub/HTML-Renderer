using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IRuleList : IEnumerable<IRule>
    {
        IRule this[int index] { get; }
        int Length { get; }
    }
}