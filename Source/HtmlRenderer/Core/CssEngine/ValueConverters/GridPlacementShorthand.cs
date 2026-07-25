using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>Shared helpers for the grid placement shorthands (<c>grid-column</c>/<c>grid-row</c>/
    /// <c>grid-area</c>): splitting a value on <c>/</c> into <c>&lt;grid-line&gt;</c> components, validating
    /// each, and building the omitted-value copy result (CSS Grid §8.3.1).</summary>
    internal static class GridPlacementShorthand
    {
        /// <summary>A parsed <c>&lt;grid-line&gt;</c> component: its authored tokens and whether it is a bare
        /// <c>&lt;custom-ident&gt;</c> (the only form the omitted-value copy rule propagates).</summary>
        internal struct Component
        {
            public IReadOnlyList<Token> Tokens { get; set; }
            public bool IsBareCustomIdent { get; set; }
        }

        private static readonly Token AutoToken = new Token(TokenType.Ident, Keywords.Auto, TextPosition.Empty);

        /// <summary>Splits the value on top-level <c>/</c> delimiters into <c>&lt;grid-line&gt;</c> components,
        /// validating each via <see cref="GridLineGrammar"/>. Returns null on any invalid component or an
        /// out-of-range component count.</summary>
        internal static List<Component> Split(IEnumerable<Token> value, int maxComponents)
        {
            var groups = new List<List<Token>> { new List<Token>() };
            foreach (var token in value)
            {
                if (token.Type == TokenType.Whitespace) { groups[groups.Count - 1].Add(token); continue; }
                if (token.Type == TokenType.Delim && token.Data == "/") { groups.Add(new List<Token>()); continue; }
                groups[groups.Count - 1].Add(token);
            }

            var components = new List<Component>(groups.Count);
            foreach (var group in groups)
            {
                var significant = group.Where(t => t.Type != TokenType.Whitespace).ToArray();
                var parsed = GridLineGrammar.TryParse(significant);
                if (parsed is null) return null;
                // Preserve internal whitespace in the stored tokens (only trim the ends) so a two-token
                // component like "span 2" round-trips as "span 2", not "span2" (which would re-lex as one
                // ident). A bare <custom-ident> is a single ident token that parsed to a named (non-Nth) line.
                components.Add(new Component
                {
                    Tokens = Trim(group),
                    IsBareCustomIdent = significant.Length == 1 && significant[0].Type == TokenType.Ident && parsed.Name != null
                });
            }

            return components.Count >= 1 && components.Count <= maxComponents ? components : null;
        }

        /// <summary>The tokens for a longhand: the specified component's tokens, or <c>auto</c> when omitted.</summary>
        internal static IReadOnlyList<Token> Or(Component? component)
        {
            return component.HasValue ? component.Value.Tokens : new[] { AutoToken };
        }

        /// <summary>Drops leading and trailing whitespace tokens, keeping internal whitespace.</summary>
        private static IReadOnlyList<Token> Trim(List<Token> group)
        {
            var start = 0;
            var end = group.Count - 1;
            while (start <= end && group[start].Type == TokenType.Whitespace) start++;
            while (end >= start && group[end].Type == TokenType.Whitespace) end--;
            return group.GetRange(start, end - start + 1);
        }
    }

    /// <summary>A shorthand <see cref="IPropertyValue"/> that emits a precomputed token stream for each of
    /// its longhands (via <see cref="ExtractFor"/>), used by the grid placement shorthands.</summary>
    internal sealed class MappedShorthandValue : IPropertyValue
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyList<Token>> _map;

        public MappedShorthandValue(IReadOnlyDictionary<string, IReadOnlyList<Token>> map, IEnumerable<Token> original)
        {
            _map = map;
            Original = new TokenValue(original);
        }

        public string CssText => Original.Text;
        public TokenValue Original { get; }

        public TokenValue ExtractFor(string name) =>
            _map.TryGetValue(name, out var tokens) ? new TokenValue(tokens) : new TokenValue(new Token[0]);
    }
}
