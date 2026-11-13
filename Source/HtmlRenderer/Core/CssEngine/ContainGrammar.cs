using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The grammars of CSS Containment 2 §2 (<c>contain</c>) and CSS Sizing 4 §5.1
    /// (<c>contain-intrinsic-*</c>). The authored text is preserved by the converters that call these.
    /// </summary>
    internal static class ContainGrammar
    {
        private const int Size = 1;
        private const int Layout = 2;
        private const int Style = 4;
        private const int Paint = 8;
        private const int InlineSize = 16;

        /// <summary>
        /// <c>none | strict | content | [ [ size | inline-size ] || layout || style || paint ]</c>.
        /// <c>none</c>, <c>strict</c> and <c>content</c> stand alone; the other keywords come in any order,
        /// each at most once, and <c>size</c> and <c>inline-size</c> not together.
        /// </summary>
        internal static bool TryParseContain(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (tokens.Length == 0 || tokens.Any(t => t.Type != TokenType.Ident)) return false;

            if (tokens.Length == 1 &&
                (tokens[0].Data.Isi(Keywords.None) || tokens[0].Data.Isi("strict") || tokens[0].Data.Isi("content")))
            {
                return true;
            }

            var seen = 0;

            foreach (var token in tokens)
            {
                var kind = KindOf(token.Data);

                if (kind == 0 || (seen & kind) != 0) return false;

                seen |= kind;
            }

            return (seen & (Size | InlineSize)) != (Size | InlineSize);
        }

        private static int KindOf(string keyword) =>
            keyword.Isi("size") ? Size
            : keyword.Isi("inline-size") ? InlineSize
            : keyword.Isi("layout") ? Layout
            : keyword.Isi("style") ? Style
            : keyword.Isi("paint") ? Paint
            : 0;

        /// <summary><c>auto? [ none | &lt;length [0,∞]&gt; ]</c> over the tokens of one value.</summary>
        internal static bool TryParseIntrinsic(IEnumerable<Token> value)
        {
            var tokens = new List<Token>(value.Where(t => t.Type != TokenType.Whitespace));

            if (tokens.Count > 0 && tokens[0].Type == TokenType.Ident && tokens[0].Data.Isi(Keywords.Auto))
            {
                tokens.RemoveAt(0);
            }

            if (tokens.Count != 1) return false;

            var token = tokens[0];

            if (token.Type == TokenType.Ident) return token.Data.Isi(Keywords.None);

            return IsNonNegativeLength(token);
        }

        /// <summary>A <c>&lt;length&gt;</c> (a bare 0 or a <c>calc()</c> included) that is not negative.</summary>
        internal static bool IsNonNegativeLength(Token token)
        {
            if (token is UnitToken { Value: < 0f }) return false;

            return Converters.LengthConverter.Convert(new[] { token }) != null;
        }
    }
}
