using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>A value validated by a grammar whose authored text is preserved verbatim.</summary>
    internal sealed class GrammarValue : IPropertyValue
    {
        public GrammarValue(IEnumerable<Token> tokens)
        {
            Original = new TokenValue(tokens);
        }

        public string CssText => Original.Text;

        public TokenValue Original { get; }

        public TokenValue ExtractFor(string name) => Original;
    }
}
