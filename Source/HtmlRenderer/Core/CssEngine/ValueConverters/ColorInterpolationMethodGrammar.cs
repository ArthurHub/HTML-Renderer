using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The grammar of a gradient's <c>&lt;color-interpolation-method&gt;</c> prelude (CSS Images 4 3.1):
    /// <c>in [ &lt;rectangular-color-space&gt; | &lt;polar-color-space&gt; &lt;hue-interpolation-method&gt;? ]</c>.
    /// The prelude combines with a gradient's own direction/shape/position using <c>||</c>, so it may appear
    /// before or after it.
    /// </summary>
    internal static class ColorInterpolationMethodGrammar
    {
        private const string In = "in";
        private const string Hue = "hue";

        private static readonly HashSet<string> ColorSpaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "srgb", "srgb-linear", "display-p3", "a98-rgb", "prophoto-rgb", "rec2020", "lab", "oklab",
            "xyz", "xyz-d65", "xyz-d50", "hsl", "hwb", "lch", "oklch"
        };

        private static readonly HashSet<string> PolarColorSpaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "hsl", "hwb", "lch", "oklch"
        };

        private static readonly HashSet<string> HueDirections = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "shorter", "longer", "increasing", "decreasing"
        };

        public static bool IsColorSpace(string name) => name != null && ColorSpaces.Contains(name);

        public static bool IsPolarColorSpace(string name) => name != null && PolarColorSpaces.Contains(name);

        public static bool IsHueDirection(string name) => name != null && HueDirections.Contains(name);

        /// <summary>
        /// If <paramref name="group"/> (one gradient comma-group's tokens) contains an <c>in</c>
        /// interpolation-method prelude, validates it and returns the group with the contiguous
        /// <c>in ...</c> slice removed as <paramref name="remainder"/> (for the caller to validate as the
        /// gradient's own direction/shape/position). Returns false when an <c>in</c> is present but the
        /// method is malformed (unknown space, a hue method on a rectangular space or without the trailing
        /// <c>hue</c>, or <c>in</c> with nothing after it). When no <c>in</c> is present,
        /// <paramref name="hasInterpolationMethod"/> is false, <paramref name="remainder"/> is the group
        /// unchanged, and it returns true.
        /// </summary>
        public static bool TryExtractInterpolationMethod(
            IEnumerable<Token> group, out List<Token> remainder, out bool hasInterpolationMethod)
        {
            var tokens = new List<Token>(group);

            var inIndex = IndexOfIdent(tokens, In);
            if (inIndex < 0)
            {
                remainder = tokens;
                hasInterpolationMethod = false;
                return true;
            }

            hasInterpolationMethod = true;

            var spaceIndex = NextNonWhitespace(tokens, inIndex + 1);
            if (spaceIndex < 0 || !IsIdent(tokens[spaceIndex], out var spaceName) || !IsColorSpace(spaceName))
            {
                remainder = null;
                return false;
            }

            var lastMethodIndex = spaceIndex;

            // A hue-interpolation method (<direction> hue) is only valid after a polar space, and is only
            // complete with the trailing "hue" keyword.
            if (IsPolarColorSpace(spaceName))
            {
                var directionIndex = NextNonWhitespace(tokens, spaceIndex + 1);
                if (directionIndex >= 0 && IsIdent(tokens[directionIndex], out var direction) &&
                    IsHueDirection(direction))
                {
                    var hueIndex = NextNonWhitespace(tokens, directionIndex + 1);
                    if (hueIndex < 0 || !IsIdent(tokens[hueIndex], out var hueWord) ||
                        !string.Equals(hueWord, Hue, StringComparison.OrdinalIgnoreCase))
                    {
                        remainder = null;
                        return false;
                    }

                    lastMethodIndex = hueIndex;
                }
            }

            remainder = BuildRemainder(tokens, inIndex, lastMethodIndex);
            return true;
        }

        // The group without tokens [first..last], whitespace-trimmed at the edges so what is left validates
        // the same as an un-prefixed group would.
        private static List<Token> BuildRemainder(List<Token> tokens, int first, int last)
        {
            var remainder = new List<Token>(tokens.Count);

            for (var i = 0; i < tokens.Count; i++)
            {
                if (i < first || i > last) remainder.Add(tokens[i]);
            }

            while (remainder.Count > 0 && remainder[0].Type == TokenType.Whitespace) remainder.RemoveAt(0);
            while (remainder.Count > 0 && remainder[remainder.Count - 1].Type == TokenType.Whitespace)
                remainder.RemoveAt(remainder.Count - 1);

            return remainder;
        }

        private static int IndexOfIdent(List<Token> tokens, string data)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                if (IsIdent(tokens[i], out var value) && string.Equals(value, data, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private static int NextNonWhitespace(List<Token> tokens, int start)
        {
            for (var i = start; i < tokens.Count; i++)
            {
                if (tokens[i].Type != TokenType.Whitespace) return i;
            }

            return -1;
        }

        private static bool IsIdent(Token token, out string data)
        {
            if (token.Type == TokenType.Ident)
            {
                data = token.Data;
                return true;
            }

            data = null;
            return false;
        }
    }
}
