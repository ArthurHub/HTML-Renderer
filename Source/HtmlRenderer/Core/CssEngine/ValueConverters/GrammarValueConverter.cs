using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A thin, text-preserving converter: accepts the value when <c>grammar</c> approves its tokens
    /// (whitespace included, so calc() stays intact) and keeps the authored text as-is. Used by the properties whose grammar lives in a
    /// static <c>XGrammar.TryParse</c> (hyphenation, hanging punctuation, rhythmic sizing, ...).
    /// </summary>
    internal sealed class GrammarValueConverter : IValueConverter
    {
        private readonly Func<IReadOnlyList<Token>, bool> _grammar;

        public GrammarValueConverter(Func<IReadOnlyList<Token>, bool> grammar)
        {
            _grammar = grammar;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.ToArray();

            return tokens.Length > 0 && _grammar(tokens) ? new GrammarValue(tokens) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GrammarValue>();
        }

        /// <summary>A grammar accepting exactly one of <paramref name="keywords"/>.</summary>
        public static GrammarValueConverter OneOf(params string[] keywords)
        {
            return new GrammarValueConverter(tokens =>
            {
                var significant = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
                return significant.Length == 1 && significant[0].Type == TokenType.Ident &&
                       keywords.Any(k => significant[0].Data.Isi(k));
            });
        }

        private sealed class GrammarValue : IPropertyValue
        {
            public GrammarValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
