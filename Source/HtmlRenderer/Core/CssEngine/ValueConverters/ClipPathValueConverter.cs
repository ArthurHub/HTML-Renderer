using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Accepts <c>none</c> or a shape <see cref="BasicShapeGrammar"/> recognizes and keeps the authored tokens as the value.
    /// </summary>
    internal sealed class ClipPathValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            var isNone = tokens.Length == 1 && tokens[0].Type == TokenType.Ident && tokens[0].Data.Isi(Keywords.None);

            if (!isNone && BasicShapeGrammar.TryParse(tokens) == null)
                return null;

            return new ClipPathValue(value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<ClipPathValue>();
        }

        private sealed class ClipPathValue : IPropertyValue
        {
            public ClipPathValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
