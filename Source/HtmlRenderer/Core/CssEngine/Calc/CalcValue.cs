using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// An accepted calc(), min(), max() or clamp() value, <see cref="CssText"/> is the folded text.
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
