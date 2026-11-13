using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Resolves the CSS color <em>function</em> forms from a tokenized value into a concrete
    /// <see cref="Color"/>: the legacy/CSS Color 4 <c>rgb()/rgba()/hsl()/hsla()/hwb()/gray()</c> and the
    /// CSS Color 4/5 <c>lab()/oklab()/lch()/oklch()/color()/color-mix()</c>. Tokenization is the object model's
    /// job, so this consumes the already-parsed <see cref="FunctionToken"/> stream (nested functions
    /// arrive as a single token) rather than re-scanning the source text.
    /// </summary>
    internal static partial class ColorFunctionExtensions
    {
        /// <summary>
        /// Fully resolves a tokenized color value to a concrete <see cref="Color"/> - named colors and
        /// hex via <see cref="ValueExtensions.ToColor"/>, plus every color <em>function</em>
        /// (rgb/hsl/hwb/gray/lab/oklab/lch/oklch/color()/color-mix/device-cmyk), including the relative
        /// color syntax of the first ones (<c>rgb(from red r g b)</c>). Unlike <c>ToColor</c> (which the object-model
        /// cascade uses and which deliberately preserves a function's specified form for serialization),
        /// this resolver always computes the sRGB value. A component written as <c>none</c> is 0 here (a
        /// missing alpha is transparent); it only stays <em>missing</em> inside a <c>color-mix()</c>.
        /// </summary>
        public static Color? ToResolvedColor(this IEnumerable<Token> value) => ResolveColor(value, wantOklab: false, out _);

        // The one resolver behind ToResolvedColor and TryResolveOklab, so a nested color-mix() is read once per level, not once per question asked of it.
        private static Color? ResolveColor(IEnumerable<Token> value, bool wantOklab, out double[] oklab)
        {
            oklab = null;
            var enumerable = value as Token[] ?? value.ToArray();

            var basic = enumerable.ToColor();
            if (basic.HasValue) return basic;

            var element = enumerable.OnlyOrDefault();

            // A hex color nested inside a function (e.g. a color-mix() operand) tokenizes as a Hash
            // keyword token rather than a ColorToken, so ToColor misses it - resolve it here.
            if (element is { Type: TokenType.Hash } hash) return Color.FromHex(hash.Data);

            return element is FunctionToken function ? ParseColorFunction(function, wantOklab, out oklab) : null;
        }

        /// <summary>
        /// Reads <c>light-dark(&lt;color&gt;, &lt;color&gt;)</c> (CSS Color 5 section 4): the tokens of its light and of its dark arm, in order. Which
        /// arm applies depends on the color scheme of the element the color is used on, which a parser does not know, so the value is not
        /// resolved to one <see cref="Color"/> (<see cref="ToResolvedColor"/> is null for it); each arm is any color, light-dark() included.
        /// False if the value is not a light-dark() of exactly two arms.
        /// </summary>
        public static bool TryGetLightDark(this IEnumerable<Token> value, out List<Token> light, out List<Token> dark)
        {
            light = dark = null;
            var element = value.OnlyOrDefault();
            return element is FunctionToken function &&
                   function.Data.Equals(FunctionNames.LightDark, StringComparison.OrdinalIgnoreCase) &&
                   TrySplitColorPair(function.ArgumentTokens, out light, out dark);
        }

        /// <summary>
        /// Reads a <c>light-dark()</c> value as the two sRGB colors its arms resolve to; an arm that is not a color
        /// <see cref="ToResolvedColor"/> resolves (currentcolor, or a nested light-dark()) is null.
        /// </summary>
        public static bool TryResolveLightDark(this IEnumerable<Token> value, out Color? light, out Color? dark)
        {
            light = dark = null;
            if (!value.TryGetLightDark(out var lightTokens, out var darkTokens)) return false;
            light = lightTokens.ToResolvedColor();
            dark = darkTokens.ToResolvedColor();
            return true;
        }

        // Exactly two comma-separated, non-empty arms (whitespace dropped).
        internal static bool TrySplitColorPair(IEnumerable<Token> args, out List<Token> first, out List<Token> second)
        {
            first = second = null;
            var arms = new List<List<Token>> { new List<Token>() };
            foreach (var token in args)
            {
                if (token.Type == TokenType.Comma) arms.Add(new List<Token>());
                else if (token.Type != TokenType.Whitespace) arms[arms.Count - 1].Add(token);
            }

            if (arms.Count != 2 || arms[0].Count == 0 || arms[1].Count == 0) return false;
            first = arms[0];
            second = arms[1];
            return true;
        }

        public static Color? ParseColorFunction(FunctionToken function) => ParseColorFunction(function, wantOklab: false, out _);

        // oklab: for lab(), lch(), oklab(), oklch(), color() and color-mix() (when wantOklab), the Oklab coordinates the color was written with, which are
        // outside the sRGB gamut where the returned Color is clipped to it; null for every other color.
        private static Color? ParseColorFunction(FunctionToken function, bool wantOklab, out double[] oklab)
        {
            oklab = null;
            var name = function.Data;
            var args = function.ArgumentTokens;

            // Relative color syntax (rgb(from red r g b) and the like) is read by whichever function it is written in.
            if (IsRelative(args)) return ParseRelative(name, args, wantOklab, out oklab);

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
                return ParseRect(args, Color.FromLab, abReference: 125.0, lReference: 100.0, ColorSpaceMath.Space.Lab, wantOklab, out oklab);
            if (name.Equals(FunctionNames.Oklab, StringComparison.OrdinalIgnoreCase))
                return ParseRect(args, Color.FromOklab, abReference: 0.4, lReference: 1.0, ColorSpaceMath.Space.Oklab, wantOklab, out oklab);
            if (name.Equals(FunctionNames.Lch, StringComparison.OrdinalIgnoreCase))
                return ParsePolar(args, Color.FromLch, chromaReference: 150.0, lReference: 100.0, ColorSpaceMath.Space.Lch, wantOklab, out oklab);
            if (name.Equals(FunctionNames.Oklch, StringComparison.OrdinalIgnoreCase))
                return ParsePolar(args, Color.FromOklch, chromaReference: 0.4, lReference: 1.0, ColorSpaceMath.Space.Oklch, wantOklab, out oklab);

            if (name.Equals(FunctionNames.ColorMix, StringComparison.OrdinalIgnoreCase))
                return ParseColorMix(args, wantOklab, out oklab);

            if (name.Equals(FunctionNames.ColorFunction, StringComparison.OrdinalIgnoreCase))
                return ParseColorSpaceFunction(args, wantOklab, out oklab);

            if (name.Equals(FunctionNames.DeviceCmyk, StringComparison.OrdinalIgnoreCase))
                return ParseDeviceCmyk(args);

            return null;
        }

        // ── device-cmyk(): 4 components (number or %) + optional alpha ───────────
        // https://www.w3.org/TR/css-color-5/#device-cmyk - alpha is only ever introduced by '/', and there is no relative ("from") form.
        private static Color? ParseDeviceCmyk(IEnumerable<Token> args)
        {
            if (!Extract(args, 4, out var comps, out var alphaTok, allowImplicitAlpha: false)) return null;
            var c = CmykComponent(comps[0]);
            var m = CmykComponent(comps[1]);
            var y = CmykComponent(comps[2]);
            var k = CmykComponent(comps[3]);
            if (c is null || m is null || y is null || k is null) return null;
            return Color.FromDeviceCmyk(c.Value, m.Value, y.Value, k.Value, Alpha(alphaTok));
        }

        // A <cmyk-component> is a <number> or <percentage>, each normalized to 0..1; none is 0.
        private static float? CmykComponent(Token token)
        {
            if (IsNone(token)) return 0f;
            var number = AsTokens(token).ToSingle();
            if (number.HasValue) return number.Value;
            return AsTokens(token).ToPercent()?.NormalizedValue;
        }

        // ── rgb()/rgba(): 3 components (int or % of 255) + optional alpha ─────────
        private static Color? ParseRgb(IEnumerable<Token> args)
        {
            if (!Extract(args, 3, out var comps, out var alphaTok)) return null;
            // none (a missing component) is only part of the space-separated syntax, not the legacy comma one.
            if (HasComma(args) && (comps.Any(IsNone) || (alphaTok != null && IsNone(alphaTok)))) return null;
            var r = RgbComponent(comps[0]);
            var g = RgbComponent(comps[1]);
            var b = RgbComponent(comps[2]);
            if (r is null || g is null || b is null) return null;
            return new Color(r.Value, g.Value, b.Value, Clamp01Byte(Alpha(alphaTok)));
        }

        // ── hsl()/hsla(): hue angle + saturation% + lightness% + optional alpha ──
        private static Color? ParseHsl(IEnumerable<Token> args)
        {
            if (!Extract(args, 3, out var comps, out var alphaTok)) return null;
            if (HasComma(args) && (comps.Any(IsNone) || (alphaTok != null && IsNone(alphaTok)))) return null;
            var hue = HueDegrees(comps[0]);
            var sat = PercentValue(comps[1]);
            var light = PercentValue(comps[2]);
            if (hue is null || sat is null || light is null) return null;
            return Color.FromHsla((float)(hue.Value / 360.0), sat.Value, light.Value, Alpha(alphaTok));
        }

        // ── hwb(): hue angle + whiteness% + blackness% + optional alpha ──────────
        private static Color? ParseHwb(IEnumerable<Token> args)
        {
            if (!Extract(args, 3, out var comps, out var alphaTok)) return null;
            var hue = HueDegrees(comps[0]);
            var white = PercentValue(comps[1]);
            var black = PercentValue(comps[2]);
            if (hue is null || white is null || black is null) return null;
            return Color.FromHwba((float)(hue.Value / 360.0), white.Value, black.Value, Alpha(alphaTok));
        }

        // ── gray(): a single lightness component + optional alpha ────────────────
        private static Color? ParseGray(IEnumerable<Token> args)
        {
            if (!Extract(args, 1, out var comps, out var alphaTok)) return null;
            var value = AsTokens(comps[0]).ToRgbComponent();
            if (value is null) return null;
            return Color.FromGray(value.Value, Alpha(alphaTok));
        }

        // ── lab()/oklab(): L + a + b + optional alpha ────────────────────────────
        private static Color? ParseRect(IEnumerable<Token> args, Func<double, double, double, float, Color> build, double abReference, double lReference,
            ColorSpaceMath.Space space, bool wantOklab, out double[] oklab)
        {
            oklab = null;
            if (!TryRectComponents(args, abReference, lReference, out var v, out var alpha, out _)) return null;
            if (wantOklab) oklab = ColorSpaceMath.ToOklab(v, space);
            return build(v[0], v[1], v[2], alpha);
        }

        private static bool TryRectComponents(IEnumerable<Token> args, double abReference, double lReference, out double[] v, out float alpha, out int missing)
        {
            v = new double[0];
            alpha = 1f;
            missing = 0;
            if (!Extract(args, 3, out var comps, out var alphaTok)) return false;
            var l = Component(comps[0], lReference);
            var a = Component(comps[1], abReference);
            var b = Component(comps[2], abReference);
            if (l is null || a is null || b is null) return false;
            alpha = Alpha(alphaTok);
            missing = MissingMask(comps, alphaTok);
            // CSS Color 4 section 9: the lightness is clamped to its range when the color is parsed (the reference is 100% of it), before any mix.
            v = new[] { Clamp(l.Value, 0, lReference), a.Value, b.Value };
            return true;
        }

        // ── lch()/oklch(): L + C + hue angle + optional alpha ────────────────────
        private static Color? ParsePolar(IEnumerable<Token> args, Func<double, double, double, float, Color> build, double chromaReference, double lReference,
            ColorSpaceMath.Space space, bool wantOklab, out double[] oklab)
        {
            oklab = null;
            if (!TryPolarComponents(args, chromaReference, lReference, out var v, out var alpha, out _)) return null;
            if (wantOklab) oklab = ColorSpaceMath.ToOklab(v, space);
            return build(v[0], v[1], v[2], alpha);
        }

        private static bool TryPolarComponents(IEnumerable<Token> args, double chromaReference, double lReference, out double[] v, out float alpha, out int missing)
        {
            v = new double[0];
            alpha = 1f;
            missing = 0;
            if (!Extract(args, 3, out var comps, out var alphaTok)) return false;
            var l = Component(comps[0], lReference);
            var c = Component(comps[1], chromaReference);
            var h = HueDegrees(comps[2]);
            if (l is null || c is null || h is null) return false;
            alpha = Alpha(alphaTok);
            missing = MissingMask(comps, alphaTok);
            // CSS Color 4 section 9: the lightness is clamped to its range and a negative chroma to 0 when the color is parsed.
            v = new[] { Clamp(l.Value, 0, lReference), Math.Max(c.Value, 0), h.Value };
            return true;
        }

        // ── color(<space> c1 c2 c3 [/ alpha]) ────────────────────────────────────
        // https://www.w3.org/TR/css-color-4/#color-function - the predefined spaces. A colour in a
        // wide-gamut space is clipped to sRGB, like the other wide-gamut functions.
        private static readonly Dictionary<string, ColorSpaceMath.Space> PredefinedSpaces =
            new Dictionary<string, ColorSpaceMath.Space>(StringComparer.OrdinalIgnoreCase)
            {
                ["srgb"] = ColorSpaceMath.Space.Srgb,
                ["srgb-linear"] = ColorSpaceMath.Space.SrgbLinear,
                ["display-p3"] = ColorSpaceMath.Space.DisplayP3,
                ["a98-rgb"] = ColorSpaceMath.Space.A98Rgb,
                ["prophoto-rgb"] = ColorSpaceMath.Space.ProphotoRgb,
                ["rec2020"] = ColorSpaceMath.Space.Rec2020,
                ["xyz"] = ColorSpaceMath.Space.XyzD65,
                ["xyz-d65"] = ColorSpaceMath.Space.XyzD65,
                ["xyz-d50"] = ColorSpaceMath.Space.XyzD50
            };

        private static Color? ParseColorSpaceFunction(IEnumerable<Token> args, bool wantOklab, out double[] oklab)
        {
            oklab = null;
            if (!TryColorFunctionComponents(args, out var space, out var v, out var alpha, out _)) return null;
            if (wantOklab) oklab = ColorSpaceMath.ToOklab(v, space);
            return Color.FromSpaceComponents(v, space, alpha);
        }

        private static bool TryColorFunctionComponents(IEnumerable<Token> args, out ColorSpaceMath.Space space, out double[] v, out float alpha, out int missing)
        {
            space = default;
            v = new double[0];
            alpha = 1f;
            missing = 0;
            // The alpha is only ever introduced by "/": a fifth bare value is malformed.
            if (!Extract(args, 4, out var comps, out var alphaTok, allowImplicitAlpha: false)) return false;
            if (comps[0].Type != TokenType.Ident || !PredefinedSpaces.TryGetValue(comps[0].Data, out space)) return false;

            // The components of every predefined space are a number or a percentage of 1 (the xyz spaces too).
            var c1 = Component(comps[1], 1.0);
            var c2 = Component(comps[2], 1.0);
            var c3 = Component(comps[3], 1.0);
            if (c1 is null || c2 is null || c3 is null) return false;
            v = new[] { c1.Value, c2.Value, c3.Value };
            alpha = Alpha(alphaTok);
            missing = MissingMask(comps.GetRange(1, 3), alphaTok);
            return true;
        }

        // ── color-mix(in <space> [<hue-method>], c1 [p1], c2 [p2]) ───────────────
        private static Color? ParseColorMix(IEnumerable<Token> args, bool wantOklab, out double[] oklab)
        {
            oklab = null;
            if (!TryColorMixOperands(args, out var c1, out var c2, out var t, out var space, out var hue, out var alphaMultiplier,
                    out var oklab1, out var oklab2, out var written1, out var written2))
                return null;

            // Both operands device-cmyk(): a mix in the C/M/Y/K components themselves (see Color.MixCmyk). One of them with an sRGB color has
            // no profile-free conversion, so it stays invalid, as does asking for the Oklab coordinates of a CMYK mix.
            if (c1.IsDeviceCmyk != c2.IsDeviceCmyk) return null;
            if (c1.IsDeviceCmyk) return Color.MixCmyk(c1, c2, t, alphaMultiplier);

            if (wantOklab)
            {
                var mixed = Color.MixComponents(c1, c2, t, space, hue, out _, oklab1, oklab2, written1, written2);
                oklab = ColorSpaceMath.ToOklab(mixed, space);
            }

            return Color.Mix(c1, c2, t, space, hue, alphaMultiplier, oklab1, oklab2, written1, written2);
        }

        /// <summary>
        /// Reads a <c>lab()</c>, <c>lch()</c>, <c>oklab()</c>, <c>oklch()</c>, <c>color()</c> or <c>color-mix()</c> color as the Oklab coordinates it names,
        /// without the clip to the sRGB gamut that <see cref="ToResolvedColor"/> applies: an out-of-gamut color keeps its out-of-range
        /// coordinates, which is what a browser mixes two colors from (CSS Color 4 section 13). False for any other color (the clipped sRGB value
        /// is the color then).
        /// </summary>
        public static bool TryResolveOklab(this IEnumerable<Token> value, out double[] oklab)
        {
            var color = ResolveColor(value, wantOklab: true, out oklab);
            if (color.HasValue && oklab != null) return true;

            oklab = new double[0];
            return false;
        }

        /// <summary>
        /// Reads any color as an operand of a mix in Oklab (an animation between two colors): its Oklab coordinates as written, without the clip to the sRGB gamut, and
        /// which of them are missing (bits 0 to 2 for L, a and b; <see cref="ColorSpaceMath.AlphaMissing"/> for the alpha) - a component written as none, carried to
        /// the analogous Oklab component where there is one (CSS Color 4 section 12.4). False for a color that does not resolve.
        /// </summary>
        public static bool TryResolveMixOperand(this IEnumerable<Token> value, out double[] oklab, out int missing)
        {
            missing = 0;
            var tokens = new List<Token>(value.Where(t => t.Type != TokenType.Whitespace));
            var color = ResolveColor(tokens, wantOklab: true, out oklab);
            if (!(color is { } resolved) || resolved.IsDeviceCmyk)
            {
                oklab = new double[0];
                return false;
            }

            oklab ??= ColorSpaceMath.ToSpace(resolved.R / 255.0, resolved.G / 255.0, resolved.B / 255.0, ColorSpaceMath.Space.Oklab);

            var written = WrittenComponents(tokens);
            if (written.HasValue)
                missing = ColorSpaceMath.MapMissing(written.Value.Space, written.Value.Missing, ColorSpaceMath.Space.Oklab);
            return true;
        }

        // The operands, weight and space of a color-mix(), after the CSS Color 5 section 3 percentage normalization.
        private static bool TryColorMixOperands(IEnumerable<Token> args, out Color c1, out Color c2, out double t, out ColorSpaceMath.Space space,
            out ColorSpaceMath.HueMethod hue, out double alphaMultiplier, out double[] oklab1, out double[] oklab2,
            out ColorSpaceMath.WrittenCoords? written1, out ColorSpaceMath.WrittenCoords? written2)
        {
            c1 = c2 = default;
            oklab1 = oklab2 = null;
            written1 = written2 = null;
            t = alphaMultiplier = 0;
            space = default;
            hue = default;

            var segments = SplitByComma(args);
            if (segments.Count != 3) return false;

            var config = segments[0].Where(x => x.Type != TokenType.Whitespace)
                .Select(x => x.Data.ToLowerInvariant()).ToList();
            if (config.Count < 2 || config[0] != "in") return false;
            space = MapSpace(config[1]);
            hue = MapHue(config, 2);

            if (!ParseMixOperand(segments[1], out c1, out var p1, out oklab1, out written1)) return false;
            if (!ParseMixOperand(segments[2], out c2, out var p2, out oklab2, out written2)) return false;

            // CSS Color 5 section 3 normalization: a missing percentage is 100% minus the other; percentages
            // scale to sum 1, and a pre-normalization sum below 100% scales the result alpha down.
            if (p1 is null && p2 is null) { p1 = 0.5; p2 = 0.5; }
            else if (p1 is null) p1 = Math.Max(0, 1.0 - p2.Value);
            else if (p2 is null) p2 = Math.Max(0, 1.0 - p1.Value);

            var sum = p1.Value + p2.Value;
            if (sum <= 0) return false;
            alphaMultiplier = sum < 1.0 ? sum : 1.0;
            t = p2.Value / sum;
            return true;
        }

        private const int MaxColorMixDepth = 32;

        [ThreadStatic]
        private static int _colorMixDepth;

        private static bool ParseMixOperand(List<Token> segment, out Color color, out double? percent, out double[] oklab, out ColorSpaceMath.WrittenCoords? written)
        {
            color = default;
            oklab = null;
            written = null;
            percent = null;

            var tokens = new List<Token>();
            foreach (var token in segment)
            {
                if (token.Type != TokenType.Whitespace) tokens.Add(token);
            }
            if (tokens.Count == 0) return false;

            // The optional percentage may appear before or after the color (CSS Color 5 section 3.1), e.g.
            // both `red 30%` and `30% red` are valid.
            if (tokens[tokens.Count - 1].Type == TokenType.Percentage)
            {
                percent = AsTokens(tokens[tokens.Count - 1]).ToPercent()?.NormalizedValue;
                tokens.RemoveAt(tokens.Count - 1);
            }
            else if (tokens[0].Type == TokenType.Percentage)
            {
                percent = AsTokens(tokens[0]).ToPercent()?.NormalizedValue;
                tokens.RemoveAt(0);
            }

            // A color-mix operand may itself be any color form (named/hex/rgb/oklch/...), so resolve it
            // through the full resolver rather than the named/hex-only ToColor. The operand comes back with
            // the coordinates it was written with too, when it is one that can lie outside the gamut the Color is clipped to.
            // color-mix() operands nest without bound in the grammar, and each level costs several frames of the machine
            // stack, so past a depth no stylesheet needs the color is invalid, as any other value that cannot be read.
            if (_colorMixDepth >= MaxColorMixDepth) return false;
            Color? resolved;
            _colorMixDepth++;
            try
            {
                resolved = ResolveColor(tokens, wantOklab: true, out oklab);
            }
            finally
            {
                _colorMixDepth--;
            }
            if (resolved is null) return false;
            color = resolved.Value;
            written = WrittenComponents(tokens);
            return true;
        }

        // The components a color operand was written with (null for any other color). Those of lab(), lch(), oklab(), oklch() and color() are always
        // read, for a gray's hue and an out-of-gamut color; the rest only when one of their components or the alpha is missing (none), as that is all a mix needs.
        private static ColorSpaceMath.WrittenCoords? WrittenComponents(List<Token> tokens)
        {
            if (tokens.Count != 1 || !(tokens[0] is FunctionToken function)) return null;

            var name = function.Data;
            var args = function.ArgumentTokens;
            double[] v;
            int missing;

            // A relative color's written components are the ones its channel keywords resolved to: a mix takes it from its Oklab coordinates.
            if (IsRelative(args)) return null;

            if (name.Equals(FunctionNames.Lab, StringComparison.OrdinalIgnoreCase) && TryRectComponents(args, 125.0, 100.0, out v, out _, out missing))
                return new ColorSpaceMath.WrittenCoords(ColorSpaceMath.Space.Lab, v, missing);
            if (name.Equals(FunctionNames.Oklab, StringComparison.OrdinalIgnoreCase) && TryRectComponents(args, 0.4, 1.0, out v, out _, out missing))
                return new ColorSpaceMath.WrittenCoords(ColorSpaceMath.Space.Oklab, v, missing);
            if (name.Equals(FunctionNames.Lch, StringComparison.OrdinalIgnoreCase) && TryPolarComponents(args, 150.0, 100.0, out v, out _, out missing))
                return new ColorSpaceMath.WrittenCoords(ColorSpaceMath.Space.Lch, v, missing);
            if (name.Equals(FunctionNames.Oklch, StringComparison.OrdinalIgnoreCase) && TryPolarComponents(args, 0.4, 1.0, out v, out _, out missing))
                return new ColorSpaceMath.WrittenCoords(ColorSpaceMath.Space.Oklch, v, missing);
            if (name.Equals(FunctionNames.ColorFunction, StringComparison.OrdinalIgnoreCase) && TryColorFunctionComponents(args, out var space, out v, out _, out missing))
                return new ColorSpaceMath.WrittenCoords(space, v, missing);

            if (!Extract(args, 3, out var comps, out var alphaTok) || HasComma(args)) return null;
            missing = MissingMask(comps, alphaTok);
            if (missing == 0) return null;

            if (name.Equals(FunctionNames.Rgb, StringComparison.OrdinalIgnoreCase) || name.Equals(FunctionNames.Rgba, StringComparison.OrdinalIgnoreCase))
            {
                var r = RgbComponent(comps[0]);
                var g = RgbComponent(comps[1]);
                var b = RgbComponent(comps[2]);
                return r is null || g is null || b is null
                    ? (ColorSpaceMath.WrittenCoords?)null
                    : new ColorSpaceMath.WrittenCoords(ColorSpaceMath.Space.Srgb, new[] { r.Value / 255.0, g.Value / 255.0, b.Value / 255.0 }, missing);
            }

            var isHsl = name.Equals(FunctionNames.Hsl, StringComparison.OrdinalIgnoreCase) || name.Equals(FunctionNames.Hsla, StringComparison.OrdinalIgnoreCase);
            if (isHsl || name.Equals(FunctionNames.Hwb, StringComparison.OrdinalIgnoreCase))
            {
                var hue = HueDegrees(comps[0]);
                var second = PercentValue(comps[1]);
                var third = PercentValue(comps[2]);
                return hue is null || second is null || third is null
                    ? (ColorSpaceMath.WrittenCoords?)null
                    : new ColorSpaceMath.WrittenCoords(isHsl ? ColorSpaceMath.Space.Hsl : ColorSpaceMath.Space.Hwb,
                        new[] { hue.Value, second.Value * 100.0, third.Value * 100.0 }, missing);
            }

            return null;
        }

        // Which of the three components (bits 0 to 2) and the alpha (ColorSpaceMath.AlphaMissing) were written as none.
        private static int MissingMask(List<Token> components, Token alpha)
        {
            var mask = 0;
            for (var i = 0; i < 3 && i < components.Count; i++)
            {
                if (IsNone(components[i])) mask |= 1 << i;
            }

            if (alpha != null && IsNone(alpha)) mask |= ColorSpaceMath.AlphaMissing;
            return mask;
        }

        // ── Token helpers ────────────────────────────────────────────────────────

        // Partitions a function's argument tokens into `count` component tokens and an optional alpha
        // token. Whitespace and commas separate; a "/" delimiter (CSS Color 4) marks the alpha that
        // follows. Without a slash, a trailing extra value (legacy rgba/hsla comma alpha) is the alpha -
        // unless allowImplicitAlpha is false (color() has no such legacy form: alpha is only ever
        // introduced by '/'), in which case an unslashed trailing value is malformed.
        private static bool Extract(IEnumerable<Token> args, int count, out List<Token> components, out Token alpha, bool allowImplicitAlpha = true)
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

            if (!allowImplicitAlpha)
            {
                if (slashIndex < 0) return values.Count == count;
                if (slashIndex < values.Count) alpha = values[slashIndex];
                return true;
            }

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

        private static Token[] AsTokens(Token token) => new[] { token };

        private static bool IsNone(Token token) =>
            token.Type == TokenType.Ident && token.Data.Equals("none", StringComparison.OrdinalIgnoreCase);

        private static bool HasComma(IEnumerable<Token> args) => args.Any(t => t.Type == TokenType.Comma);

        private static double Clamp(double value, double min, double max) => value < min ? min : (value > max ? max : value);

        // A missing component (none) is 0 once the color is used for anything but a mix.
        private static byte? RgbComponent(Token token) => IsNone(token) ? (byte)0 : AsTokens(token).ToRgbComponent();

        private static float? PercentValue(Token token)
        {
            if (IsNone(token)) return 0f;
            return AsTokens(token).ToPercent()?.NormalizedValue;
        }

        // The alpha of a color: a number or percentage; a missing alpha (none) is transparent once the color is used for anything but a mix.
        private static float Alpha(Token token)
        {
            if (token is null) return 1f;
            return IsNone(token) ? 0f : AsTokens(token).ToAlphaValue() ?? 1f;
        }

        // A number (returned as-is) or a percentage resolved against `reference`; "none" resolves to 0.
        private static double? Component(Token token, double reference)
        {
            if (IsNone(token)) return 0.0;
            var number = AsTokens(token).ToSingle();
            if (number.HasValue) return number.Value;
            var percent = AsTokens(token).ToPercent();
            return percent.HasValue ? percent.Value.NormalizedValue * reference : (double?)null;
        }

        private static double? HueDegrees(Token token)
        {
            if (IsNone(token)) return 0.0;
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
            var clamped = alpha < 0f ? 0f : (alpha > 1f ? 1f : alpha);
            return (byte)Math.Round(clamped * 255);
        }

        private static ColorSpaceMath.Space MapSpace(string name)
        {
            switch (name)
            {
                case "srgb-linear": return ColorSpaceMath.Space.SrgbLinear;
                case "display-p3": return ColorSpaceMath.Space.DisplayP3;
                case "a98-rgb": return ColorSpaceMath.Space.A98Rgb;
                case "prophoto-rgb": return ColorSpaceMath.Space.ProphotoRgb;
                case "rec2020": return ColorSpaceMath.Space.Rec2020;
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
                switch (idents[i])
                {
                    case "longer": return ColorSpaceMath.HueMethod.Longer;
                    case "increasing": return ColorSpaceMath.HueMethod.Increasing;
                    case "decreasing": return ColorSpaceMath.HueMethod.Decreasing;
                    default: return ColorSpaceMath.HueMethod.Shorter;
                }
            }
            return ColorSpaceMath.HueMethod.Shorter;
        }
    }
}
