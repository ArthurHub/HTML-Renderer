using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <see cref="IPropertyValue"/> produced by <see cref="CalcValueConverter"/> for an accepted
    /// calc()/min()/max()/clamp() declaration. Mirrors StructValueConverter's wrapper: CssText is the
    /// canonical (possibly folded) text, Original is the raw token stream.
    /// </summary>
    internal sealed class CalcValue : IPropertyValue
    {
        public CalcValue(CalcNode node, CalcCategory category, IEnumerable<Token> tokens)
        {
            Node = node;
            Category = category;
            CssText = CalcSerializer.Serialize(node, category);
            Original = new TokenValue(tokens);
        }

        internal CalcNode Node { get; }

        internal CalcCategory Category { get; }

        public string CssText { get; }

        public TokenValue Original { get; }

        public TokenValue ExtractFor(string name)
        {
            return Original;
        }
    }
}
