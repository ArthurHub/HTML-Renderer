using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-template-areas</c>, CSS Grid 2 §7.3.</summary>
    internal sealed class GridTemplateAreasValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            var isNone = tokens.Length == 1 && tokens[0].Type == TokenType.Ident && tokens[0].Data.Isi(Keywords.None);

            if (!isNone && GridTemplateAreasGrammar.TryParse(tokens) == null)
                return null;

            return new GridTemplateAreasValue(value);
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<GridTemplateAreasValue>();

        private sealed class GridTemplateAreasValue : IPropertyValue
        {
            public GridTemplateAreasValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;
            public TokenValue Original { get; }
            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
