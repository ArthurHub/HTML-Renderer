using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Resolves the CSS color <em>function</em> forms from a tokenized value into a concrete
    /// <see cref="Color"/>: the legacy/CSS Color 4 <c>rgb()/rgba()/hsl()/hsla()/hwb()/gray()</c> and the
    /// CSS Color 4/5 <c>lab()/oklab()/lch()/oklch()/color-mix()</c>. Tokenization is the CSS-OM's job, so
    /// this consumes the already-parsed <see cref="FunctionToken"/> stream (nested functions arrive as a
    /// single token) rather than re-scanning the source text.
    /// </summary>
    internal static class ColorFunctionExtensions
    {
        /// <summary>
        /// Fully resolves a tokenized color value to a concrete <see cref="Color"/> - named colors and
        /// hex via <see cref="ValueExtensions.ToColor"/>, plus every color <em>function</em>
        /// (rgb/hsl/hwb/gray/lab/oklab/lch/oklch/color-mix). Unlike <c>ToColor</c> (which the CSS-OM
        /// cascade uses and which deliberately preserves a function's specified form for serialization),
        /// this is the render-layer resolver: it always computes the sRGB value.
        /// </summary>
        public static Color? ToResolvedColor(this IEnumerable<Token> value)
        {
            var basic = value.ToColor();
            if (basic.HasValue) return basic;

            var element = value.OnlyOrDefault();

            // A hex color nested inside a function (e.g. a color-mix() operand) tokenizes as a Hash
            // keyword token rather than a ColorToken, so ToColor misses it - resolve it here.
            if (element != null && element.Type == TokenType.Hash) return Color.FromHex(element.Data);

            var function = element as FunctionToken;
            return function != null ? ParseColorFunction(function) : null;
        }

        public static Color? ParseColorFunction(FunctionToken function)
        {
            var name = function.Data;
            var args = function.ArgumentTokens;

            if (name.Equals(FunctionNames.Rgb, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(FunctionNames.Rgba, StringComparison.OrdinalIgnoreCase))
                return ParseRgb(args);

            if (name.Equals(FunctionNames.Hsl, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(FunctionNames.Hsla, StringComparison.OrdinalIgnoreCase))
                return ParseHsl(args);

            if (name.Equals(FunctionNames.Hwb, StringComparison.OrdinalIgnoreCase))
                return ParseHwb(args);

            if (name.Equals(FunctionNames.Gray, StringComparison.OrdinalIgnoreCase))
                return ParseGray(args);

            if (name.Equals(FunctionNames.Lab, StringComparison.OrdinalIgnoreCase))
                return ParseRect(args, Color.FromLab, abReference: 125.0, lReference: 100.0);
            if (name.Equals(FunctionNames.Oklab, StringComparison.OrdinalIgnoreCase))
                return ParseRect(args, Color.FromOklab, abReference: 0.4, lReference: 1.0);
            if (name.Equals(FunctionNames.Lch, StringComparison.OrdinalIgnoreCase))
                return ParsePolar(args, Color.FromLch, chromaReference: 150.0, lReference: 100.0);
            if (name.Equals(FunctionNames.Oklch, StringComparison.OrdinalIgnoreCase))
                return ParsePolar(args, Color.FromOklch, chromaReference: 0.4, lReference: 1.0);

            if (name.Equals(FunctionNames.ColorMix, StringComparison.OrdinalIgnoreCase))
                return ParseColorMix(args);

            return null;
        }

        // rgb()/rgba(): 3 components (int or % of 255) + optional alpha
        private static Color? ParseRgb(IEnumerable<Token> args)
        {
            List<Token> comps;
            Token alphaTok;
            if (!Extract(args, 3, out comps, out alphaTok)) return null;
            var r = AsTokens(comps[0]).ToRgbComponent();
            var g = AsTokens(comps[1]).ToRgbComponent();
            var b = AsTokens(comps[2]).ToRgbComponent();
            if (r == null || g == null || b == null) return null;
            var a = alphaTok == null ? 1f : AsTokens(alphaTok).ToAlphaValue() ?? 1f;
            return new Color(r.Value, g.Value, b.Value, Clamp01Byte(a));
        }

        // hsl()/hsla(): hue angle + saturation% + lightness% + optional alpha
        private static Color? ParseHsl(IEnumerable<Token> args)
        {
            List<Token> comps;
            Token alphaTok;
            if (!Extract(args, 3, out comps, out alphaTok)) return null;
            var hue = HueDegrees(comps[0]);
            var sat = AsTokens(comps[1]).ToPercent();
            var light = AsTokens(comps[2]).ToPercent();
            if (hue == null || sat == null || light == null) return null;
            var a = alphaTok == null ? 1f : AsTokens(alphaTok).ToAlphaValue() ?? 1f;
            return Color.FromHsla((float)(hue.Value / 360.0), sat.Value.NormalizedValue, light.Value.NormalizedValue, a);
        }

        // hwb(): hue angle + whiteness% + blackness% + optional alpha
        private static Color? ParseHwb(IEnumerable<Token> args)
        {
            List<Token> comps;
            Token alphaTok;
            if (!Extract(args, 3, out comps, out alphaTok)) return null;
            var hue = HueDegrees(comps[0]);
            var white = AsTokens(comps[1]).ToPercent();
            var black = AsTokens(comps[2]).ToPercent();
            if (hue == null || white == null || black == null) return null;
            var a = alphaTok == null ? 1f : AsTokens(alphaTok).ToAlphaValue() ?? 1f;
            return Color.FromHwba((float)(hue.Value / 360.0), white.Value.NormalizedValue, black.Value.NormalizedValue, a);
        }

        // gray(): a single lightness component + optional alpha
        private static Color? ParseGray(IEnumerable<Token> args)
        {
            List<Token> comps;
            Token alphaTok;
            if (!Extract(args, 1, out comps, out alphaTok)) return null;
            var value = AsTokens(comps[0]).ToRgbComponent();
            if (value == null) return null;
            var a = alphaTok == null ? 1f : AsTokens(alphaTok).ToAlphaValue() ?? 1f;
            return Color.FromGray(value.Value, a);
        }

        // lab()/oklab(): L + a + b + optional alpha
        private static Color? ParseRect(IEnumerable<Token> args, Func<double, double, double, float, Color> build, double abReference, double lReference)
        {
            List<Token> comps;
            Token alphaTok;
            if (!Extract(args, 3, out comps, out alphaTok)) return null;
            var l = Component(comps[0], lReference);
            var a = Component(comps[1], abReference);
            var b = Component(comps[2], abReference);
            if (l == null || a == null || b == null) return null;
            var alpha = alphaTok == null ? 1f : AsTokens(alphaTok).ToAlphaValue() ?? 1f;
            return build(l.Value, a.Value, b.Value, alpha);
        }

        // lch()/oklch(): L + C + hue angle + optional alpha
        private static Color? ParsePolar(IEnumerable<Token> args, Func<double, double, double, float, Color> build, double chromaReference, double lReference)
        {
            List<Token> comps;
            Token alphaTok;
            if (!Extract(args, 3, out comps, out alphaTok)) return null;
            var l = Component(comps[0], lReference);
            var c = Component(comps[1], chromaReference);
            var h = HueDegrees(comps[2]);
            if (l == null || c == null || h == null) return null;
            var alpha = alphaTok == null ? 1f : AsTokens(alphaTok).ToAlphaValue() ?? 1f;
            return build(l.Value, c.Value, h.Value, alpha);
        }

        // color-mix(in <space> [<hue-method>], c1 [p1], c2 [p2])
        private static Color? ParseColorMix(IEnumerable<Token> args)
        {
            var segments = SplitByComma(args);
            if (segments.Count != 3) return null;

            var config = segments[0].Where(token => token.Type != TokenType.Whitespace)
                .Select(token => token.Data.ToLowerInvariant()).ToList();
            if (config.Count < 2 || config[0] != "in") return null;
            var space = MapSpace(config[1]);
            var hue = MapHue(config, 2);

            Color c1, c2;
            double? p1, p2;
            if (!ParseMixOperand(segments[1], out c1, out p1)) return null;
            if (!ParseMixOperand(segments[2], out c2, out p2)) return null;

            // CSS Color 5 §3 normalization: a missing percentage is 100% minus the other; percentages
            // scale to sum 1, and a pre-normalization sum below 100% scales the result alpha down.
            if (p1 == null && p2 == null) { p1 = 0.5; p2 = 0.5; }
            else if (p1 == null) p1 = Math.Max(0, 1.0 - p2.Value);
            else if (p2 == null) p2 = Math.Max(0, 1.0 - p1.Value);

            var sum = p1.Value + p2.Value;
            if (sum <= 0) return null;
            var alphaMultiplier = sum < 1.0 ? sum : 1.0;
            var t = p2.Value / sum;

            return Color.Mix(c1, c2, t, space, hue, alphaMultiplier);
        }

        private static bool ParseMixOperand(List<Token> segment, out Color color, out double? percent)
        {
            color = default(Color);
            percent = null;

            var tokens = new List<Token>();
            foreach (var token in segment)
            {
                if (token.Type != TokenType.Whitespace) tokens.Add(token);
            }
            if (tokens.Count == 0) return false;

            // The optional percentage may appear before or after the color (CSS Color 5 §3.1), e.g.
            // both `red 30%` and `30% red` are valid.
            if (tokens[tokens.Count - 1].Type == TokenType.Percentage)
            {
                var value = AsTokens(tokens[tokens.Count - 1]).ToPercent();
                percent = value.HasValue ? value.Value.NormalizedValue : (double?)null;
                tokens.RemoveAt(tokens.Count - 1);
            }
            else if (tokens[0].Type == TokenType.Percentage)
            {
                var value = AsTokens(tokens[0]).ToPercent();
                percent = value.HasValue ? value.Value.NormalizedValue : (double?)null;
                tokens.RemoveAt(0);
            }

            // A color-mix operand may itself be any color form (named/hex/rgb/oklch/...), so resolve it
            // through the full render-layer resolver rather than the named/hex-only ToColor.
            var resolved = tokens.ToResolvedColor();
            if (resolved == null) return false;
            color = resolved.Value;
            return true;
        }

        // Token helpers

        // Partitions a function's argument tokens into `count` component tokens and an optional alpha
        // token. Whitespace and commas separate; a "/" delimiter (CSS Color 4) marks the alpha that
        // follows. Without a slash, a trailing extra value (legacy rgba/hsla comma alpha) is the alpha.
        private static bool Extract(IEnumerable<Token> args, int count, out List<Token> components, out Token alpha)
        {
            components = new List<Token>();
            alpha = null;

            var values = new List<Token>();
            var slashIndex = -1;
            foreach (var token in args)
            {
                switch (token.Type)
                {
                    case TokenType.Whitespace:
                    case TokenType.Comma:
                        continue;
                    case TokenType.Delim when token.Data == "/":
                        slashIndex = values.Count;
                        continue;
                    default:
                        values.Add(token);
                        break;
                }
            }

            if (values.Count < count) return false;
            for (var i = 0; i < count; i++) components.Add(values[i]);

            var alphaAt = slashIndex >= 0 ? slashIndex : count;
            if (alphaAt < values.Count) alpha = values[alphaAt];
            return true;
        }

        private static List<List<Token>> SplitByComma(IEnumerable<Token> tokens)
        {
            var result = new List<List<Token>>();
            var current = new List<Token>();
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Comma)
                {
                    result.Add(current);
                    current = new List<Token>();
                }
                else
                {
                    current.Add(token);
                }
            }
            result.Add(current);
            return result;
        }

        private static Token[] AsTokens(Token token)
        {
            return new[] { token };
        }

        // A number (returned as-is) or a percentage resolved against `reference`; "none" resolves to 0.
        private static double? Component(Token token, double reference)
        {
            if (token.Type == TokenType.Ident && token.Data.Equals("none", StringComparison.OrdinalIgnoreCase))
                return 0.0;
            var number = AsTokens(token).ToSingle();
            if (number.HasValue) return number.Value;
            var percent = AsTokens(token).ToPercent();
            return percent.HasValue ? percent.Value.NormalizedValue * reference : (double?)null;
        }

        private static double? HueDegrees(Token token)
        {
            if (token.Type == TokenType.Ident && token.Data.Equals("none", StringComparison.OrdinalIgnoreCase))
                return 0.0;
            var angle = AsTokens(token).ToAngleNumber();
            if (!angle.HasValue) return null;

            switch (angle.Value.Type)
            {
                case Angle.Unit.Grad: return angle.Value.Value * 0.9;
                case Angle.Unit.Turn: return angle.Value.Value * 360.0;
                case Angle.Unit.Rad: return angle.Value.Value * 180.0 / Math.PI;
                default: return angle.Value.Value;
            }
        }

        private static byte Clamp01Byte(float alpha)
        {
            return (byte)Math.Round(Math.Min(Math.Max(alpha, 0f), 1f) * 255);
        }

        private static ColorSpaceMath.Space MapSpace(string name)
        {
            switch (name)
            {
                case "srgb-linear": return ColorSpaceMath.Space.SrgbLinear;
                case "display-p3": return ColorSpaceMath.Space.DisplayP3;
                case "lab": return ColorSpaceMath.Space.Lab;
                case "oklab": return ColorSpaceMath.Space.Oklab;
                case "xyz":
                case "xyz-d65": return ColorSpaceMath.Space.XyzD65;
                case "xyz-d50": return ColorSpaceMath.Space.XyzD50;
                case "hsl": return ColorSpaceMath.Space.Hsl;
                case "hwb": return ColorSpaceMath.Space.Hwb;
                case "lch": return ColorSpaceMath.Space.Lch;
                case "oklch": return ColorSpaceMath.Space.Oklch;
                default: return ColorSpaceMath.Space.Srgb;
            }
        }

        private static ColorSpaceMath.HueMethod MapHue(List<string> idents, int startIndex)
        {
            for (var i = startIndex; i + 1 < idents.Count; i++)
            {
                if (idents[i + 1] != "hue") continue;
                return MapHueName(idents[i]);
            }
            return ColorSpaceMath.HueMethod.Shorter;
        }

        private static ColorSpaceMath.HueMethod MapHueName(string name)
        {
            switch (name)
            {
                case "longer": return ColorSpaceMath.HueMethod.Longer;
                case "increasing": return ColorSpaceMath.HueMethod.Increasing;
                case "decreasing": return ColorSpaceMath.HueMethod.Decreasing;
                default: return ColorSpaceMath.HueMethod.Shorter;
            }
        }

        /// <summary>
        /// Blends two colors per the CSS <c>palette-mix()</c> rules (CSS Fonts 4), reusing <c>color-mix()</c>'s
        /// color-space mapping (<see cref="MapSpace"/>) and CSS Color 5 §3 percentage normalization so the two
        /// functions interpolate identically. <paramref name="p1"/>/<paramref name="p2"/> are 0..100 percentages
        /// (or null); <paramref name="hueName"/> is a hue-interpolation direction (or null for the default).
        /// </summary>
        internal static Color MixPaletteColors(Color c1, Color c2, string spaceName, string hueName, double? p1, double? p2)
        {
            // CSS keywords are case-insensitive; MapSpace/MapHueName match lowercase literals, so fold first
            // (a mixed-case space would otherwise silently fall through to the sRGB/Shorter default).
            var space = MapSpace(spaceName.ToLowerInvariant());
            var hue = MapHueName(hueName.ToLowerInvariant());

            double? f1 = p1.HasValue ? Math.Min(Math.Max(p1.Value, 0), 100) / 100.0 : (double?)null;
            double? f2 = p2.HasValue ? Math.Min(Math.Max(p2.Value, 0), 100) / 100.0 : (double?)null;

            if (f1 == null && f2 == null) { f1 = 0.5; f2 = 0.5; }
            else if (f1 == null) f1 = 1.0 - f2;
            else if (f2 == null) f2 = 1.0 - f1;

            var sum = f1.Value + f2.Value;
            if (sum <= 0) return c1;

            var alphaMultiplier = sum < 1.0 ? sum : 1.0;
            var t = f2.Value / sum;
            return Color.Mix(c1, c2, t, space, hue, alphaMultiplier);
        }
    }
}
