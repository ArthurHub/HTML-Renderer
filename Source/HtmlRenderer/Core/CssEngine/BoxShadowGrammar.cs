using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Grammar of a <c>box-shadow</c> value, CSS Backgrounds 3 §6.1. Keeps the components as authored
    /// strings, em/rem lengths can only be resolved against the box.
    /// </summary>
    internal static class BoxShadowGrammar
    {
        /// <summary>
        /// One shadow layer. <see cref="Blur"/> and <see cref="Spread"/> default to <c>"0"</c>, a null
        /// <see cref="Color"/> means currentcolor.
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

        private static readonly IValueConverter ColorValidator = Converters.ColorConverter.WithCurrentColor();

        /// <summary>
        /// The layers in declaration order, an empty list for <c>none</c>, or null if the value is invalid.
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
        /// Outside of a declaration value the tokenizer gives a hex color as a bare <see cref="TokenType.Hash"/>
        /// token instead of a <see cref="ColorToken"/>, which <see cref="ColorValidator"/> doesn't accept.
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
        /// Outside of a declaration value the tokenizer splits a hex color starting with a digit (<c>#08f</c>)
        /// into a <c>#</c> delimiter and a number or dimension, this merges them back into one hash token.
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
