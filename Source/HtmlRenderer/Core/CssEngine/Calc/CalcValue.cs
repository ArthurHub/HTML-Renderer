using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The value produced by <see cref="CalcValueConverter"/> for an accepted calc()/min()/max()/clamp()
    /// expression. Keeps the parsed <see cref="Node"/> and its type-checked <see cref="Category"/>;
    /// <see cref="CssText"/> is the canonical text from <see cref="CalcSerializer"/> (folded to a plain
    /// value where the expression needs no layout context), and <see cref="Original"/> the raw tokens.
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

        public CalcNode Node { get; }

        public CalcCategory Category { get; }

        public string CssText { get; }

        public TokenValue Original { get; }

        public TokenValue ExtractFor(string name)
        {
            return Original;
        }
    }
}
