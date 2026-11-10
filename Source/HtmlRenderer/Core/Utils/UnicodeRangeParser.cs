#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Parses the <c>unicode-range</c> descriptor of <c>@font-face</c> (CSS Fonts 4 §4.5) into inclusive
    /// codepoint ranges.
    /// </summary>
    public static class UnicodeRangeParser
    {
        /// <summary>
        /// Returns null when the descriptor is absent or has no valid range, the face is then used for
        /// any character.
        /// </summary>
        public static IReadOnlyList<CodepointRange>? Parse(string? descriptor)
        {
            if (string.IsNullOrWhiteSpace(descriptor))
                return null;

            List<CodepointRange>? ranges = null;

            // The value may come without its "U+" prefixes ("41-5A, 61-7A"), the tokenizer needs them.
            // The ! is for netstandard2.0, whose IsNullOrWhiteSpace has no [NotNullWhen].
            foreach (var segment in descriptor!.Split(','))
            {
                var normalized = segment.Trim();

                if (normalized.Length == 0)
                    continue;

                if (!normalized.StartsWith("U+", StringComparison.OrdinalIgnoreCase))
                    normalized = "U+" + normalized;

                var rangeToken = CssValueParser.GetCssTokens(normalized).OfType<RangeToken>().FirstOrDefault();

                if (rangeToken == null)
                    continue;

                if (!int.TryParse(rangeToken.Start, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var start) ||
                    !int.TryParse(rangeToken.End, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var end))
                    continue;

                if (start > Symbols.MaximumCodepoint)
                    continue;

                if (end > Symbols.MaximumCodepoint)
                    end = Symbols.MaximumCodepoint;

                if (end < start)
                    continue;

                // Surrogates aren't characters, a bound on one moves past them.
                if (start is >= 0xD800 and <= 0xDFFF)
                    start = 0xE000;
                if (end is >= 0xD800 and <= 0xDFFF)
                    end = 0xD7FF;
                if (start > end)
                    continue;

                (ranges ??= new List<CodepointRange>()).Add(new CodepointRange(start, end));
            }

            return ranges;
        }

        /// <summary>
        /// Whether <paramref name="codepoint"/> falls inside any of <paramref name="ranges"/>.
        /// </summary>
        public static bool Covers(IReadOnlyList<CodepointRange> ranges, int codepoint)
        {
            for (var i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].Contains(codepoint))
                    return true;
            }
            return false;
        }
    }
}
