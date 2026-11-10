using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// <c>grid-template-columns</c>/<c>-rows</c>, CSS Grid 2 §7.2. The parsed template is kept as a typed
    /// <see cref="CssProperty{T}"/>.
    /// </summary>
    internal sealed class GridTemplateValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            var isNone = tokens.Length == 1 && tokens[0].Type == TokenType.Ident && tokens[0].Data.Isi(Keywords.None);

            GridTemplate template = null;
            if (!isNone)
            {
                template = GridTrackListGrammar.TryParse(tokens);
                if (template is null) return null;
            }

            return new GridTemplateValue(value, template);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GridTemplateValue>();
        }

        /// <summary>
        /// For the cascade paths that only have a string: the initial value, a CSS-wide keyword, an inherited
        /// value or a resolved <c>var()</c>.
        /// </summary>
        internal static CssProperty<GridTemplate> FromCssText(string value)
        {
            if (CssGlobalKeywords.TryParse(value, out var keyword))
                return CssProperty<GridTemplate>.Global(keyword);
            if (value.IndexOf("var(", StringComparison.OrdinalIgnoreCase) >= 0)
                return CssProperty<GridTemplate>.Unresolved(value);
            var template = GridTrackListGrammar.TryParse(Tokenize(value));
            return CssProperty<GridTemplate>.FromValue(value, template);
        }

        /// <summary>Like <c>CssValueParser.GetCssTokens</c>, without depending on <c>Core.Parse</c>.</summary>
        private static List<Token> Tokenize(string value)
        {
            using (var lexer = new Lexer(value) { IsInValue = false })
            {
                var tokens = new List<Token>();
                Token token;
                do
                {
                    token = lexer.Get();
                    if (token.Type != TokenType.EndOfFile && token.Type != TokenType.Whitespace)
                        tokens.Add(token);
                } while (token.Type != TokenType.EndOfFile);
                return tokens;
            }
        }

        private sealed class GridTemplateValue : IPropertyValue, ITypedPropertyValue<GridTemplate>
        {
            private readonly CssProperty<GridTemplate> _typed;

            public GridTemplateValue(IEnumerable<Token> tokens, GridTemplate template)
            {
                Original = new TokenValue(tokens);
                // null template for none
                _typed = CssProperty<GridTemplate>.FromValue(Original.Text, template);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;

            public CssProperty<GridTemplate> GetTypedValue() => _typed;
        }
    }
}
