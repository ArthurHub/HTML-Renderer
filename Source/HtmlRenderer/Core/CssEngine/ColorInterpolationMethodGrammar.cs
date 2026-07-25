using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The single, shared grammar for a gradient's <c>&lt;color-interpolation-method&gt;</c> prelude
    /// (<a href="https://drafts.csswg.org/css-images-4/#color-interpolation-method">CSS Images 4 §3.1</a>):
    /// <code>in [ &lt;rectangular-color-space&gt; | &lt;polar-color-space&gt; &lt;hue-interpolation-method&gt;? ]</code>.
    /// Both the CSS-OM validator (<see cref="GradientConverter"/>, which drops a malformed prelude at
    /// parse time) and the render-time parser classify keywords through this one class, so the two
    /// layers cannot disagree about which preludes are valid.
    ///
    /// Only the color spaces the interpolation math actually implements are recognized; the
    /// wide-gamut RGB spaces <c>a98-rgb</c>/<c>prophoto-rgb</c>/<c>rec2020</c> are valid CSS but are
    /// deliberately rejected here rather than approximated.
    /// </summary>
    internal static class ColorInterpolationMethodGrammar
    {
        private const string In = "in";
        private const string Hue = "hue";

        private static readonly HashSet<string> ColorSpaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "srgb", "srgb-linear", "display-p3", "lab", "oklab", "xyz", "xyz-d65", "xyz-d50",
            "hsl", "hwb", "lch", "oklch",
        };

        private static readonly HashSet<string> PolarColorSpaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "hsl", "hwb", "lch", "oklch",
        };

        private static readonly HashSet<string> HueDirections = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "shorter", "longer", "increasing", "decreasing",
        };

        public static bool IsColorSpace(string name)
        {
            return name != null && ColorSpaces.Contains(name);
        }

        public static bool IsPolarColorSpace(string name)
        {
            return name != null && PolarColorSpaces.Contains(name);
        }

        public static bool IsHueDirection(string name)
        {
            return name != null && HueDirections.Contains(name);
        }

        /// <summary>
        /// If <paramref name="group"/> (one gradient comma-group's tokens) contains an <c>in</c>
        /// interpolation-method prelude, validates it and returns the group with the contiguous
        /// <c>in …</c> slice removed as <paramref name="remainder"/> (for the caller to validate as the
        /// gradient's own direction/shape/position, since the two combine with <c>||</c> in either order).
        /// Returns <see langword="false"/> when an <c>in</c> is present but the method is malformed
        /// (unknown/unsupported space, a hue-interpolation method on a rectangular space or without the
        /// trailing <c>hue</c>, or <c>in</c> with nothing after it) — the caller drops the declaration.
        /// When no <c>in</c> is present, <paramref name="hasInterpolationMethod"/> is
        /// <see langword="false"/>, <paramref name="remainder"/> is the group unchanged, and it returns
        /// <see langword="true"/>.
        /// </summary>
        public static bool TryExtractInterpolationMethod(
            IEnumerable<Token> group, out List<Token> remainder, out bool hasInterpolationMethod)
        {
            var tokens = new List<Token>(group);

            int inIndex = IndexOfIdent(tokens, 0, In);
            if (inIndex < 0)
            {
                remainder = tokens;
                hasInterpolationMethod = false;
                return true;
            }

            hasInterpolationMethod = true;

            // in <color-space>
            int spaceIndex = NextNonWhitespace(tokens, inIndex + 1);
            string spaceName;
            if (spaceIndex < 0 || !IsIdent(tokens[spaceIndex], out spaceName) || !IsColorSpace(spaceName))
            {
                remainder = null;
                return false;
            }

            int lastMethodIndex = spaceIndex;

            // A hue-interpolation method (<dir> hue) is only valid after a polar space, and only complete
            // with the trailing "hue" keyword.
            if (IsPolarColorSpace(spaceName))
            {
                int dirIndex = NextNonWhitespace(tokens, spaceIndex + 1);
                string dir;
                if (dirIndex >= 0 && IsIdent(tokens[dirIndex], out dir) && IsHueDirection(dir))
                {
                    int hueIndex = NextNonWhitespace(tokens, dirIndex + 1);
                    string hueWord;
                    if (hueIndex < 0 || !IsIdent(tokens[hueIndex], out hueWord) ||
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

        // The group with tokens [firstMethodIndex..lastMethodIndex] removed, then whitespace-trimmed at the
        // edges so the leftover direction/shape validates the same as an un-prefixed group would.
        private static List<Token> BuildRemainder(List<Token> tokens, int firstMethodIndex, int lastMethodIndex)
        {
            var remainder = new List<Token>(tokens.Count);

            for (int i = 0; i < tokens.Count; i++)
            {
                if (i >= firstMethodIndex && i <= lastMethodIndex) continue;
                remainder.Add(tokens[i]);
            }

            while (remainder.Count > 0 && remainder[0].Type == TokenType.Whitespace)
                remainder.RemoveAt(0);
            while (remainder.Count > 0 && remainder[remainder.Count - 1].Type == TokenType.Whitespace)
                remainder.RemoveAt(remainder.Count - 1);

            return remainder;
        }

        private static int IndexOfIdent(List<Token> tokens, int start, string data)
        {
            for (int i = start; i < tokens.Count; i++)
            {
                string value;
                if (IsIdent(tokens[i], out value) &&
                    string.Equals(value, data, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private static int NextNonWhitespace(List<Token> tokens, int start)
        {
            for (int i = start; i < tokens.Count; i++)
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
