using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates a <c>grid-template-columns</c>/<c>grid-template-rows</c> value: accepts the keyword
    /// <c>none</c>, the <c>subgrid</c> keyword, or any <c>&lt;track-list&gt;</c> the shared
    /// <see cref="GridTrackListGrammar"/> recognizes (lengths, %, <c>fr</c>, <c>auto</c>,
    /// <c>min-content</c>/<c>max-content</c>, <c>minmax()</c>, <c>fit-content()</c>, <c>repeat()</c>),
    /// rejecting everything else so an invalid template is dropped at parse time. The parse produced here to
    /// validate is <b>kept</b> and exposed as a <see cref="CssProperty{T}"/> via
    /// <see cref="ITypedPropertyValue{T}"/>, so the grid layout engine (Layer B) reads the already-parsed
    /// template instead of re-parsing the same string.
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
        /// Builds the box-side <see cref="CssProperty{T}"/> for a grid-template value directly from its authored
        /// string, for the cascade paths that apply a plain string rather than a Layer A parse result (the
        /// initial-value seed, a resolved global keyword, an inherited copy, a var()-resolved string). A CSS-wide
        /// keyword becomes a global value; a string still containing <c>var(</c> stays unresolved (never parsed
        /// into a bogus template); otherwise the track list is parsed by the same <see cref="GridTrackListGrammar"/>
        /// the converter uses — keeping all <see cref="CssProperty{T}"/> construction in the CSS-OM layer.
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

        /// <summary>Tokenizes an authored value string with the CSS-OM <see cref="Lexer"/> (whitespace/EOF
        /// stripped) — the CSS-OM equivalent of the Html-layer <c>CssValueParser.GetCssTokens</c>, so this
        /// Layer A factory has no upward dependency on <c>Html.Core.Parse</c>.</summary>
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
                // template is null for the `none` keyword — a resolved value with a null GridTemplate, which
                // the engine treats as an empty (no explicit tracks) grid, exactly like the string path.
                _typed = CssProperty<GridTemplate>.FromValue(Original.Text, template);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;

            public CssProperty<GridTemplate> GetTypedValue() => _typed;
        }
    }
}
