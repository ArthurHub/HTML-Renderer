using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>Helpers for <c>grid-row</c>, <c>grid-column</c> and <c>grid-area</c>, CSS Grid 2 §8.4.</summary>
    internal static class GridPlacementShorthand
    {
        /// <summary>A <c>&lt;grid-line&gt;</c>, only a bare <c>&lt;custom-ident&gt;</c> is copied to omitted lines.</summary>
        internal struct Component
        {
            public IReadOnlyList<Token> Tokens { get; set; }
            public bool IsBareCustomIdent { get; set; }
        }

        private static readonly Token AutoToken = new Token(TokenType.Ident, Keywords.Auto, TextPosition.Empty);

        /// <summary>Splits on <c>/</c>, null if a component is invalid or there are too many.</summary>
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
                // Keep the inner whitespace, "span2" would lex as one ident.
                components.Add(new Component
                {
                    Tokens = Trim(group),
                    IsBareCustomIdent = significant.Length == 1 && significant[0].Type == TokenType.Ident && parsed.Name != null
                });
            }

            return components.Count >= 1 && components.Count <= maxComponents ? components : null;
        }

        internal static IReadOnlyList<Token> Or(Component? component)
        {
            return component.HasValue ? component.Value.Tokens : new[] { AutoToken };
        }

        private static IReadOnlyList<Token> Trim(List<Token> group)
        {
            var start = 0;
            var end = group.Count - 1;
            while (start <= end && group[start].Type == TokenType.Whitespace) start++;
            while (end >= start && group[end].Type == TokenType.Whitespace) end--;
            return group.GetRange(start, end - start + 1);
        }
    }

    /// <summary>A shorthand value with precomputed tokens per longhand.</summary>
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
