using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>aspect-ratio</c> grammar (CSS Sizing 4 §4.1) and the <c>&lt;ratio&gt;</c> type (CSS Values 4 §5.7).
    /// </summary>
    internal static class AspectRatioGrammar
    {
        /// <summary>
        /// <paramref name="ratio"/> is width / height, or null for a bare <c>auto</c> or a degenerate ratio.
        /// </summary>
        internal static bool TryParse(IReadOnlyList<Token> tokens, out double? ratio) =>
            TryParse(tokens, out ratio, out _);

        /// <summary>
        /// Like <see cref="TryParse(IReadOnlyList{Token}, out double?)"/>, <paramref name="hasAuto"/> tells
        /// <c>auto &lt;ratio&gt;</c>, where a replaced element's natural ratio wins, from a bare <c>&lt;ratio&gt;</c>.
        /// </summary>
        internal static bool TryParse(IReadOnlyList<Token> tokens, out double? ratio, out bool hasAuto)
        {
            ratio = null;
            hasAuto = false;

            Token[] toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (toks.Length == 0) return false;

            var i = 0;

            if (IsAuto(toks[i])) { hasAuto = true; i++; }

            if (i == toks.Length) return hasAuto; // bare `auto`

            // <ratio> = <number> [ / <number> ]?
            if (!TryRatioCore(toks, ref i, out ratio)) return false;

            // An optional trailing `auto` (the `||` allows either order: `<ratio> auto`).
            if (i < toks.Length && !hasAuto && IsAuto(toks[i])) { hasAuto = true; i++; }

            if (i != toks.Length) return false; // trailing junk

            return true;
        }

        /// <summary>
        /// A bare <c>&lt;ratio&gt;</c> without <c>auto</c>, for the <c>@property</c> syntax matcher.
        /// </summary>
        internal static bool TryParseRatio(IReadOnlyList<Token> tokens, out double? ratio)
        {
            ratio = null;

            Token[] toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

            var i = 0;
            if (!TryRatioCore(toks, ref i, out ratio)) return false;
            return i == toks.Length; // no trailing junk (and, since we never consume `auto`, no `auto`)
        }

        private static bool TryRatioCore(Token[] toks, ref int i, out double? ratio)
        {
            ratio = null;

            // <number> [ / <number> ]?
            if (i >= toks.Length || !TryNonNegativeNumber(toks[i], out var width)) return false;
            double height = 1;
            i++;

            if (i < toks.Length && IsSlash(toks[i]))
            {
                i++;
                if (i >= toks.Length || !TryNonNegativeNumber(toks[i], out height)) return false;
                i++;
            }

            // A zero term degenerates to "no preferred aspect ratio".
            ratio = width <= 0 || height <= 0 ? (double?)null : width / height;
            return true;
        }

        private static bool IsAuto(Token token) => token.Type == TokenType.Ident && token.Data.Isi(Keywords.Auto);

        private static bool IsSlash(Token token) => token.Type == TokenType.Delim && token.Data == "/";

        private static bool TryNonNegativeNumber(Token token, out double value)
        {
            if (token is NumberToken number && number.Value >= 0f)
            {
                value = number.Value;
                return true;
            }

            value = 0;
            return false;
        }
    }
}
