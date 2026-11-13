using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The grammar for <c>hyphenate-limit-chars</c> (CSS Text 4 6.3.4): <c>[ auto | &lt;integer [0,inf]&gt; ]{1,3}</c>.
    /// The first value is the minimum word length, the second the minimum characters before the break, the third the
    /// minimum after it. A missing third value takes the second's value; a missing second value is <c>auto</c>.
    /// </summary>
    internal static class HyphenateLimitCharsGrammar
    {
        /// <summary>
        /// Validates the tokens. Each out parameter is null for <c>auto</c> (or an omitted trailing value), otherwise
        /// the parsed non-negative minimum.
        /// </summary>
        internal static bool TryParse(IEnumerable<Token> tokens, out int? wordMin, out int? beforeMin, out int? afterMin)
        {
            wordMin = null;
            beforeMin = null;
            afterMin = null;

            var toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (toks.Length == 0 || toks.Length > 3) return false;

            if (!TryValue(toks[0], out wordMin)) return false;

            if (toks.Length >= 2 && !TryValue(toks[1], out beforeMin)) return false;

            if (toks.Length == 3)
            {
                if (!TryValue(toks[2], out afterMin)) return false;
            }
            else
            {
                afterMin = beforeMin;
            }

            return true;
        }

        internal static bool IsValid(IReadOnlyList<Token> tokens) => TryParse(tokens, out _, out _, out _);

        private static bool TryValue(Token token, out int? value)
        {
            if (token.Type == TokenType.Ident && token.Data.Isi(Keywords.Auto))
            {
                value = null;
                return true;
            }

            if (token is NumberToken number && number.IsInteger && number.IntegerValue >= 0)
            {
                value = number.IntegerValue;
                return true;
            }

            value = null;
            return false;
        }
    }
}
