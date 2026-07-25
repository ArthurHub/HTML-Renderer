using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The shared grammar for the <c>aspect-ratio</c> value (<c>[ auto || &lt;ratio&gt; ]</c>, where
    /// <c>&lt;ratio&gt; = &lt;number [0,∞]&gt; [ / &lt;number [0,∞]&gt; ]?</c>). Used by both Layer A (to
    /// validate/accept-or-reject at parse time) and Layer B (to compute the used ratio during layout), so the
    /// grammar is defined once — the <see cref="BasicShapeGrammar"/> precedent.
    /// </summary>
    internal static class AspectRatioGrammar
    {
        /// <summary>
        /// Validates the token stream as an <c>aspect-ratio</c> value. Returns false for anything that is not a
        /// valid value. On success, <paramref name="ratio"/> is the used width/height ratio, or null when there
        /// is no usable ratio (a bare <c>auto</c>, or a ratio with a zero term — both of which mean "no
        /// preferred aspect ratio" for a non-replaced box).
        /// </summary>
        internal static bool TryParse(IReadOnlyList<Token> tokens, out double? ratio) =>
            TryParse(tokens, out ratio, out _);

        /// <summary>
        /// Validates the token stream as an <c>aspect-ratio</c> value, additionally reporting whether the
        /// <c>auto</c> keyword was present (<c>[ auto || &lt;ratio&gt; ]</c>). <paramref name="hasAuto"/> lets a
        /// replaced element prefer its natural aspect ratio and fall back to the specified <c>&lt;ratio&gt;</c>
        /// (CSS Box Sizing 4 §5): <c>auto &lt;ratio&gt;</c> ⇒ <c>hasAuto=true</c> with a usable
        /// <paramref name="ratio"/>; a bare <c>&lt;ratio&gt;</c> ⇒ <c>hasAuto=false</c> (the ratio always
        /// overrides); a bare <c>auto</c> ⇒ <c>hasAuto=true</c> with a null <paramref name="ratio"/>.
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
        /// Validates a bare <c>&lt;ratio&gt;</c> value (<c>&lt;number [0,∞]&gt; [ / &lt;number [0,∞]&gt; ]?</c>),
        /// with no <c>auto</c> — the CSS Values 4 <c>&lt;ratio&gt;</c> data type, as used by the <c>@property</c>
        /// <c>syntax: "&lt;ratio&gt;"</c> matcher. Distinct from <see cref="TryParse(IReadOnlyList{Token}, out double?)"/>, whose <c>aspect-ratio</c>
        /// grammar additionally permits <c>auto</c>. On success <paramref name="ratio"/> is the used width/height
        /// ratio, or null when a term is zero.
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
