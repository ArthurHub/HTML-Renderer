using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The value grammars of the <c>font-variant-*</c> longhands (CSS Fonts 4 6) and of the
    /// <c>font-variant</c> shorthand that combines them.
    /// </summary>
    internal static class FontVariantGrammar
    {
        public static readonly string[] CapsKeywords =
        {
            Keywords.Normal, "small-caps", "all-small-caps", "petite-caps", "all-petite-caps", "unicase",
            "titling-caps"
        };

        public static readonly string[] PositionKeywords = {Keywords.Normal, "sub", "super"};

        public static readonly string[] EmojiKeywords = {Keywords.Normal, "text", "emoji", "unicode"};

        // Each inner array is one mutually exclusive axis. The "||" combinator allows every component at
        // most once, so a second keyword from an axis already seen (the same one repeated or its opposite)
        // makes the value invalid.
        private static readonly string[][] LigatureAxes =
        {
            new[] {"common-ligatures", "no-common-ligatures"},
            new[] {"discretionary-ligatures", "no-discretionary-ligatures"},
            new[] {"historical-ligatures", "no-historical-ligatures"},
            new[] {"contextual", "no-contextual"}
        };

        private static readonly string[][] NumericAxes =
        {
            new[] {"lining-nums", "oldstyle-nums"},
            new[] {"proportional-nums", "tabular-nums"},
            new[] {"diagonal-fractions", "stacked-fractions"},
            new[] {"ordinal"},
            new[] {"slashed-zero"}
        };

        private static readonly string[][] EastAsianAxes =
        {
            new[] {"jis78-forms", "jis83-forms", "jis90-forms", "jis04-forms", "simplified", "traditional"},
            new[] {"full-width", "proportional-width"},
            new[] {"ruby"}
        };

        public static bool TryParseCaps(IReadOnlyList<Token> tokens, out string text) =>
            FontGrammar.TryParseKeyword(tokens, CapsKeywords, out text);

        public static bool TryParsePosition(IReadOnlyList<Token> tokens, out string text) =>
            FontGrammar.TryParseKeyword(tokens, PositionKeywords, out text);

        public static bool TryParseEmoji(IReadOnlyList<Token> tokens, out string text) =>
            FontGrammar.TryParseKeyword(tokens, EmojiKeywords, out text);

        /// <summary><c>normal | none | [ &lt;common-lig-values&gt; || ... ]</c></summary>
        public static bool TryParseLigatures(IReadOnlyList<Token> tokens, out string text) =>
            TryParseKeywordsOrAxes(tokens, LigatureAxes, true, out text);

        /// <summary><c>normal | [ &lt;numeric-figure-values&gt; || ... ]</c></summary>
        public static bool TryParseNumeric(IReadOnlyList<Token> tokens, out string text) =>
            TryParseKeywordsOrAxes(tokens, NumericAxes, false, out text);

        /// <summary><c>normal | [ &lt;east-asian-variant-values&gt; || &lt;east-asian-width-values&gt; || ruby ]</c></summary>
        public static bool TryParseEastAsian(IReadOnlyList<Token> tokens, out string text) =>
            TryParseKeywordsOrAxes(tokens, EastAsianAxes, false, out text);

        private static bool TryParseKeywordsOrAxes(IReadOnlyList<Token> tokens, string[][] axes, bool allowNone,
            out string text)
        {
            text = null;

            if (tokens.Count == 1 && FontGrammar.IsIdent(tokens[0], Keywords.Normal))
            {
                text = Keywords.Normal;
                return true;
            }

            if (allowNone && tokens.Count == 1 && FontGrammar.IsIdent(tokens[0], Keywords.None))
            {
                text = Keywords.None;
                return true;
            }

            return TryParseAxes(tokens, axes, out text);
        }

        private static bool TryParseAxes(IReadOnlyList<Token> tokens, string[][] axes, out string text)
        {
            text = null;

            if (tokens.Count == 0) return false;

            var seen = new HashSet<int>();
            var keywords = new List<string>();

            foreach (var token in tokens)
            {
                if (token.Type != TokenType.Ident) return false;

                var axis = System.Array.FindIndex(axes, a => FontGrammar.Contains(a, token.Data));

                if (axis < 0 || !seen.Add(axis)) return false;

                keywords.Add(token.Data.ToLowerInvariant());
            }

            text = string.Join(" ", keywords);
            return true;
        }

        private enum Slot
        {
            Ligatures,
            Alternates,
            Caps,
            Numeric,
            EastAsian,
            Position,
            Emoji
        }

        /// <summary>
        /// The <c>font-variant</c> shorthand: <c>normal | none | [ &lt;caps&gt; || &lt;ligatures&gt; ||
        /// &lt;alternates&gt; || &lt;numeric&gt; || &lt;east-asian&gt; || &lt;position&gt; || &lt;emoji&gt; ]</c>, the
        /// components in any order. <c>normal</c> resets every longhand to <c>normal</c>; <c>none</c> sets
        /// <c>font-variant-ligatures</c> to <c>none</c> and every other longhand to <c>normal</c>. Longhands a
        /// component list does not mention are <c>null</c> in <paramref name="texts"/> and
        /// <paramref name="tokens"/> (they reset to their initial value). Both arrays follow the slot order
        /// of <see cref="FontVariantShorthandValueConverter.Longhands"/>.
        /// </summary>
        public static bool TryParseShorthand(IReadOnlyList<Token> input, out string[] texts, out List<Token>[] tokens)
        {
            texts = null;
            tokens = null;

            if (input.Count == 0) return false;

            var resultTexts = new string[7];
            var resultTokens = new List<Token>[7];

            if (input.Count == 1 && input[0].Type == TokenType.Ident &&
                (input[0].Data.Isi(Keywords.Normal) || input[0].Data.Isi(Keywords.None)))
            {
                var none = input[0].Data.Isi(Keywords.None);

                for (var i = 0; i < resultTexts.Length; i++)
                {
                    var keyword = none && i == (int) Slot.Ligatures ? Keywords.None : Keywords.Normal;
                    resultTexts[i] = keyword;
                    resultTokens[i] = new List<Token> {new Token(TokenType.Ident, keyword, TextPosition.Empty)};
                }

                texts = resultTexts;
                tokens = resultTokens;
                return true;
            }

            foreach (var token in input)
            {
                var slot = Classify(token);

                if (slot < 0) return false;

                // Caps, position and emoji each take a single keyword; the other slots accumulate tokens.
                var list = resultTokens[slot] ??= new List<Token>();

                if (slot == (int) Slot.Caps || slot == (int) Slot.Position || slot == (int) Slot.Emoji)
                {
                    if (list.Count > 0) return false;
                }
                else if (list.Count > 0)
                {
                    list.Add(Token.Whitespace);
                }

                list.Add(token);
            }

            for (var slot = 0; slot < resultTokens.Length; slot++)
            {
                var list = resultTokens[slot];

                if (list == null) continue;

                var significant = list.Where(t => t.Type != TokenType.Whitespace).ToArray();
                FontGrammarParser parser = slot switch
                {
                    (int) Slot.Ligatures => TryParseLigatures,
                    (int) Slot.Alternates => FontVariantAlternatesGrammar.TryParse,
                    (int) Slot.Numeric => TryParseNumeric,
                    (int) Slot.EastAsian => TryParseEastAsian,
                    (int) Slot.Caps => TryParseCaps,
                    (int) Slot.Position => TryParsePosition,
                    _ => TryParseEmoji
                };

                if (!parser(significant, out resultTexts[slot])) return false;
            }

            texts = resultTexts;
            tokens = resultTokens;
            return true;
        }

        // normal/none are only valid as the sole component, so any occurrence here classifies as invalid.
        private static int Classify(Token token)
        {
            if (token.Type == TokenType.Function) return FontVariantAlternatesGrammar.IsAlternatesToken(token) ? (int) Slot.Alternates : -1;

            if (token.Type != TokenType.Ident) return -1;

            var name = token.Data;

            if (name.Isi(Keywords.Normal) || name.Isi(Keywords.None)) return -1;

            if (FontGrammar.Contains(CapsKeywords, name)) return (int) Slot.Caps;
            if (FontGrammar.Contains(PositionKeywords, name)) return (int) Slot.Position;
            if (FontGrammar.Contains(EmojiKeywords, name)) return (int) Slot.Emoji;
            if (FontVariantAlternatesGrammar.IsAlternatesToken(token)) return (int) Slot.Alternates;
            if (LigatureAxes.Any(axis => FontGrammar.Contains(axis, name))) return (int) Slot.Ligatures;
            if (NumericAxes.Any(axis => FontGrammar.Contains(axis, name))) return (int) Slot.Numeric;
            if (EastAsianAxes.Any(axis => FontGrammar.Contains(axis, name))) return (int) Slot.EastAsian;

            return -1;
        }
    }
}
