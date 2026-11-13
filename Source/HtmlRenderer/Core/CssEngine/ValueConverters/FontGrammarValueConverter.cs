using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Parses the whitespace-free token list of a value and, when it is valid, produces the canonical
    /// serialization of that value.
    /// </summary>
    internal delegate bool FontGrammarParser(IReadOnlyList<Token> tokens, out string text);

    /// <summary>
    /// A thin, text-preserving wrapper that validates a value against one of the hand-written font
    /// grammars (<see cref="FontVariantGrammar"/>, <see cref="FontFeatureTagListGrammar"/>, ...) and keeps
    /// the grammar's canonical text as the value. Mirrors <see cref="AspectRatioValueConverter"/>.
    /// </summary>
    internal sealed class FontGrammarValueConverter : IValueConverter
    {
        private readonly FontGrammarParser _parser;

        public FontGrammarValueConverter(FontGrammarParser parser)
        {
            _parser = parser;
        }

        /// <summary>A converter accepting exactly one of the given identifiers (any case).</summary>
        public static IValueConverter ForKeywords(params string[] keywords)
        {
            return new FontGrammarValueConverter((IReadOnlyList<Token> tokens, out string text) =>
                FontGrammar.TryParseKeyword(tokens, keywords, out text));
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var all = value.ToArray();
            var significant = all.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (significant.Length == 0) return null;

            return _parser(significant, out var text) ? new GrammarValue(text, all) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GrammarValue>();
        }

        private sealed class GrammarValue : IPropertyValue
        {
            public GrammarValue(string text, IEnumerable<Token> tokens)
            {
                CssText = text;
                Original = new TokenValue(tokens);
            }

            public string CssText { get; }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }

    /// <summary>Small token helpers shared by the font grammars.</summary>
    internal static class FontGrammar
    {
        public static bool IsIdent(Token token, string keyword) =>
            token.Type == TokenType.Ident && token.Data.Isi(keyword);

        public static bool Contains(string[] set, string value) =>
            Array.FindIndex(set, s => s.Isi(value)) >= 0;

        /// <summary>Exactly one identifier from <paramref name="keywords"/>; the text is lower-cased.</summary>
        public static bool TryParseKeyword(IReadOnlyList<Token> tokens, string[] keywords, out string text)
        {
            text = null;

            if (tokens.Count != 1 || tokens[0].Type != TokenType.Ident || !Contains(keywords, tokens[0].Data))
                return false;

            text = tokens[0].Data.ToLowerInvariant();
            return true;
        }

        /// <summary>An OpenType tag: a string of exactly four printable ASCII characters.</summary>
        public static bool IsOpenTypeTag(Token token)
        {
            return token.Type == TokenType.String && token.Data.Length == 4 && token.Data.All(IsPrintableAscii);
        }

        public static bool IsPrintableAscii(char c) => c >= 0x20 && c <= 0x7E;

        public static string Quote(string value) => "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
