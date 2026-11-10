using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-row-start</c>/<c>-end</c>, <c>grid-column-start</c>/<c>-end</c>, CSS Grid 2 §8.3.</summary>
    internal sealed class GridLineValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.ToArray();
            return GridLineGrammar.TryParse(tokens.Where(t => t.Type != TokenType.Whitespace).ToArray()) is null
                ? null
                : new GridLineValue(tokens);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GridLineValue>();
        }

        private sealed class GridLineValue : IPropertyValue
        {
            public GridLineValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
