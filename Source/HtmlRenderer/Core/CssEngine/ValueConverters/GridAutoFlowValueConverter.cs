using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates a <c>grid-auto-flow</c> value (CSS Grid §7.7): <c>[ row | column ] || dense</c> — one of
    /// <c>row</c>/<c>column</c> optionally combined with <c>dense</c>, in either order. The authored text is
    /// preserved for the grid layout engine.
    /// </summary>
    internal sealed class GridAutoFlowValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.ToArray();
            var idents = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (idents.Length < 1 || idents.Length > 2 || idents.Any(t => t.Type != TokenType.Ident))
                return null;

            var hasAxis = false;
            var hasDense = false;
            foreach (var t in idents)
            {
                if (t.Data.Isi(Keywords.Row) || t.Data.Isi(Keywords.Column))
                {
                    if (hasAxis) return null; // row and column are mutually exclusive
                    hasAxis = true;
                }
                else if (t.Data.Isi(Keywords.Dense))
                {
                    if (hasDense) return null;
                    hasDense = true;
                }
                else
                {
                    return null;
                }
            }

            return new GridAutoFlowValue(tokens);
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<GridAutoFlowValue>();

        private sealed class GridAutoFlowValue : IPropertyValue
        {
            public GridAutoFlowValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;
            public TokenValue Original { get; }
            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
