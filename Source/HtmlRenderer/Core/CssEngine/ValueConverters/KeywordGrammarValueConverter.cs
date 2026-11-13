using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Accepts a value whose significant tokens satisfy a keyword grammar, and preserves the authored text.
    /// </summary>
    internal sealed class KeywordGrammarValueConverter : IValueConverter
    {
        private readonly Func<Token[], bool> _isValid;

        public KeywordGrammarValueConverter(Func<Token[], bool> isValid)
        {
            _isValid = isValid;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            return tokens.Length != 0 && _isValid(tokens) ? new KeywordGrammarValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<KeywordGrammarValue>();
        }

        private sealed class KeywordGrammarValue : IPropertyValue
        {
            public KeywordGrammarValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
