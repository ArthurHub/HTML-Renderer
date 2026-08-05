using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Shared, layer-agnostic grammar for the CSS <c>box-shadow</c> value (CSS Backgrounds &amp; Borders
    /// Level 3 §5): a comma-separated list of shadow layers, each
    /// <c>[ inset? &amp;&amp; &lt;length&gt;{2,4} &amp;&amp; &lt;color&gt;? ]</c>. Like
    /// <see cref="BasicShapeGrammar"/> / <see cref="AspectRatioGrammar"/>, it validates the grammar and
    /// captures each layer's structure as <b>raw component strings</b> (never resolved numbers), so both
    /// Layer A (the CSS-OM converter, which only needs to accept/reject and preserve the authored text)
    /// and Layer B (the paint-time resolver in <c>TheArtOfDev.HtmlRenderer.Core</c>, which resolves each length
    /// against the box's own font/size) share a single parser rather than re-implementing the grammar.
    /// </summary>
    internal static class BoxShadowGrammar
    {
        /// <summary>
        /// A single parsed shadow layer. Lengths and the color are kept as the authored component strings -
        /// box-shadow lengths may be font-relative (<c>em</c>/<c>rem</c>), so only Layer B can resolve them.
        /// <see cref="Blur"/> defaults to <c>"0"</c> and <see cref="Spread"/> to <c>"0"</c> when the author
        /// omitted them; <see cref="Color"/> is null when omitted, meaning "use the element's own color"
        /// (currentColor) at paint time.
        /// </summary>
        internal sealed class ShadowLayer
        {
            public bool Inset { get; set; }
            public string OffsetX { get; set; }
            public string OffsetY { get; set; }
            public string Blur { get; set; }
            public string Spread { get; set; }
            public string Color { get; set; }
        }

        // The one color grammar (named/hex/rgb()/rgba()/hsl()/currentColor/transparent), reused so box-shadow
        // never re-implements color parsing - matches the CLAUDE.md "one grammar per value" rule.
        private static readonly IValueConverter ColorValidator = Converters.ColorConverter.WithCurrentColor();

        /// <summary>
        /// Parses a <c>box-shadow</c> value's tokens into an ordered list of <see cref="ShadowLayer"/>s
        /// (first-declared first), or returns null when the value is not a valid box-shadow. The literal
        /// keyword <c>none</c> returns an <b>empty list</b> (no shadows), distinct from a null (invalid)
        /// result.
        /// </summary>
        internal static List<ShadowLayer> TryParse(IReadOnlyList<Token> tokens)
        {
            var significant = NormalizeHexColorTokens(tokens.Where(t => t.Type != TokenType.Whitespace).ToArray());

            if (significant.Count == 0) return null;

            if (significant.Count == 1 && significant[0].Type == TokenType.Ident && significant[0].Data.Isi(Keywords.None))
                return new List<ShadowLayer>();

            var layers = new List<ShadowLayer>();

            foreach (var group in SplitByComma(significant))
            {
                if (group.Count == 0) return null; // leading/trailing/doubled comma

                var layer = ParseLayer(group);
                if (layer is null) return null;

                layers.Add(layer);
            }

            return layers;
        }

        private static ShadowLayer ParseLayer(IReadOnlyList<Token> group)
        {
            var insetSeen = false;
            var lengths = new List<Token>();
            var colorTokens = new List<Token>();
            var lengthsClosed = false; // set once a non-length token appears after lengths have begun

            foreach (var token in group)
            {
                if (IsInset(token))
                {
                    if (insetSeen) return null;          // "inset" at most once
                    insetSeen = true;
                    if (lengths.Count > 0) lengthsClosed = true;
                }
                else if (IsLength(token))
                {
                    if (lengthsClosed) return null;      // <length>{2,4} must be one contiguous run
                    lengths.Add(token);
                }
                else
                {
                    if (lengths.Count > 0) lengthsClosed = true;
                    colorTokens.Add(token);
                }
            }

            // Two to four lengths: offset-x, offset-y, [blur], [spread].
            if (lengths.Count < 2 || lengths.Count > 4) return null;

            // Blur radius (the third length) must be non-negative.
            if (lengths.Count >= 3 && LengthValue(lengths[2]) < 0) return null;

            // Any color present must be a single valid <color>.
            string color = null;
            if (colorTokens.Count > 0)
            {
                if (!IsValidColor(colorTokens)) return null;
                color = string.Concat(colorTokens.Select(t => t.ToValue()));
            }

            return new ShadowLayer
            {
                Inset = insetSeen,
                OffsetX = lengths[0].ToValue(),
                OffsetY = lengths[1].ToValue(),
                Blur = lengths.Count >= 3 ? lengths[2].ToValue() : "0",
                Spread = lengths.Count >= 4 ? lengths[3].ToValue() : "0",
                Color = color,
            };
        }

        /// <summary>
        /// Validates the color component. The full color grammar (named/rgb()/rgba()/hsl()/currentColor)
        /// is checked via the shared <see cref="ColorValidator"/>, but a hex color is special-cased: the
        /// raw <c>CssValueParser.GetCssTokens</c> tokenizer Layer B feeds this grammar produces a bare
        /// <see cref="TokenType.Hash"/> token for <c>#fff</c> (only the in-declaration-value tokenizer path
        /// yields a <see cref="ColorToken"/> the converter understands), so accept a well-formed hex token
        /// directly - otherwise a hex shadow color accepted by Layer A would be dropped at paint time.
        /// </summary>
        private static bool IsValidColor(IReadOnlyList<Token> colorTokens)
        {
            if (colorTokens.Count == 1 && colorTokens[0].Type == TokenType.Hash)
                return IsHexColor(colorTokens[0].Data);

            return ColorValidator.Convert(colorTokens) != null;
        }

        private static bool IsHexColor(string data)
        {
            var length = data.Length;
            return (length == 3 || length == 4 || length == 6 || length == 8) && data.All(Uri.IsHexDigit);
        }

        /// <summary>
        /// Merges a <c>#</c> delimiter followed by a number/dimension back into one hex-color token. The
        /// <c>CssValueParser.GetCssTokens</c> tokenizer Layer B feeds this grammar is not in "value" mode,
        /// so a <b>digit-leading</b> hex color (<c>#08f</c>, <c>#000</c>) is split into a bare
        /// <see cref="TokenType.Delim"/> <c>#</c> plus a <see cref="TokenType.Number"/>/<see cref="TokenType.Dimension"/>
        /// (e.g. <c>08f</c> becomes a dimension "08" + unit "f"), whereas a letter-leading hex (<c>#c00</c>)
        /// becomes a single <see cref="TokenType.Hash"/> token. Without this, a digit-leading hex shadow color
        /// accepted by Layer A (whose value-mode tokenizer yields a proper color token) would fail to re-parse
        /// at paint time and the shadow would silently vanish.
        /// </summary>
        private static List<Token> NormalizeHexColorTokens(IReadOnlyList<Token> tokens)
        {
            var result = new List<Token>(tokens.Count);

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                if (token.Type == TokenType.Delim && token.Data == "#" && i + 1 < tokens.Count
                    && (tokens[i + 1].Type == TokenType.Number || tokens[i + 1].Type == TokenType.Dimension)
                    && IsHexColor(tokens[i + 1].ToValue()))
                {
                    result.Add(new KeywordToken(TokenType.Hash, tokens[i + 1].ToValue(), token.Position));
                    i++;
                    continue;
                }

                result.Add(token);
            }

            return result;
        }

        private static bool IsInset(Token token) =>
            token.Type == TokenType.Ident && token.Data.Isi(Keywords.Inset);

        /// <summary>A box-shadow offset/blur/spread is a <c>&lt;length&gt;</c> (not a length-percentage):
        /// a dimension, or the unitless zero. Percentages are not valid here.</summary>
        private static bool IsLength(Token token)
        {
            if (token.Type == TokenType.Dimension) return true;
            var number = token as NumberToken;
            return number != null && number.Value == 0f;
        }

        private static float LengthValue(Token token)
        {
            var unit = token as UnitToken;
            if (unit != null) return unit.Value;

            var number = token as NumberToken;
            if (number != null) return number.Value;

            return 0f;
        }

        private static List<List<Token>> SplitByComma(IReadOnlyList<Token> tokens)
        {
            var groups = new List<List<Token>>();
            var current = new List<Token>();

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Comma)
                {
                    groups.Add(current);
                    current = new List<Token>();
                }
                else
                {
                    current.Add(token);
                }
            }

            groups.Add(current);
            return groups;
        }
    }
}
