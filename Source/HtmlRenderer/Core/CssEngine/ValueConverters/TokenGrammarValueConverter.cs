using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A thin, text-preserving converter over a static grammar: the whitespace-free tokens are handed to
    /// <c>accepts</c>, and the authored text is kept when it returns true.
    /// </summary>
    internal sealed class TokenGrammarValueConverter : IValueConverter
    {
        private readonly Func<IReadOnlyList<Token>, bool> _accepts;

        public TokenGrammarValueConverter(Func<IReadOnlyList<Token>, bool> accepts)
        {
            _accepts = accepts;
        }

        /// <summary>A converter accepting exactly one of the given identifiers (case-insensitive).</summary>
        public static IValueConverter OneOf(params string[] keywords)
        {
            return new TokenGrammarValueConverter(tokens =>
                tokens.Count == 1 && tokens[0].Type == TokenType.Ident &&
                keywords.Any(k => tokens[0].Data.Isi(k)));
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = Significant(value);

            return tokens.Count != 0 && _accepts(tokens) ? new GrammarValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GrammarValue>();
        }

        internal static List<Token> Significant(IEnumerable<Token> tokens)
        {
            return new List<Token>(tokens.Where(t => t.Type != TokenType.Whitespace));
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
