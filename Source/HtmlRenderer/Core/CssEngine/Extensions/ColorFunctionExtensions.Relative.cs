using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    // Relative color syntax (CSS Color 5 section 4): rgb(from red r g b), hsl(from ...), hwb(from ...), lab(from ...), lch(from ...), oklab(from ...),
    // oklch(from ...) and color(from ... <space> ...). The origin color is converted to the space of the function it is written in, its components
    // become channel keywords (r g b, h s l, l a b, ... and alpha) that stand for numbers, and each component of the result is then a number, a percentage,
    // none, a keyword, or a calc() over keywords.
    internal static partial class ColorFunctionExtensions
    {
        private static bool IsRelative(IEnumerable<Token> args)
        {
            foreach (var token in args)
            {
                if (token.Type == TokenType.Whitespace) continue;
                return token.Type == TokenType.Ident && token.Data.Equals("from", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static Color? ParseRelative(string name, IEnumerable<Token> args, bool wantOklab, out double[] oklab)
        {
            oklab = null;

            var tokens = new List<Token>(args.Where(t => t.Type != TokenType.Whitespace));
            // from <color> <components> [/ alpha]: no commas, and the origin is a single token (a nested function arrives as one).
            if (tokens.Count < 5 || tokens.Any(t => t.Type == TokenType.Comma)) return null;

            if (!Relative.TryGetSpec(name.ToLowerInvariant(), out var spec)) return null;

            // Exactly the components, then optionally a slash and one alpha.
            var rest = new List<Token>(tokens.Skip(2));
            var slash = rest.FindIndex(t => t.Type == TokenType.Delim && t.Data == "/");
            if (slash < 0 ? rest.Count != spec.ComponentCount : slash != spec.ComponentCount || rest.Count != spec.ComponentCount + 2) return null;

            if (!Extract(rest, spec.ComponentCount, out var comps, out var alphaTok, allowImplicitAlpha: false)) return null;
            if (spec.SpaceToken && !Relative.TryReadSpace(comps[0], out spec)) return null;

            // color-mix() operands and relative origins nest without bound in the grammar; they share one depth limit.
            if (_colorMixDepth >= MaxColorMixDepth) return null;
            Color? origin;
            double[] originOklab;
            _colorMixDepth++;
            try
            {
                origin = ResolveColor(new[] { tokens[1] }, wantOklab: true, out originOklab);
            }
            finally
            {
                _colorMixDepth--;
            }

            // A device-cmyk() origin has no sRGB value to take channels from.
            if (!origin.HasValue || origin.Value.IsDeviceCmyk) return null;
            var originColor = origin.Value;

            var coords = Relative.OriginCoordinates(originColor, originOklab, spec.Space);
            var variables = new Dictionary<string, double>(4);
            for (var i = 0; i < 3; i++) variables[spec.Keywords[i]] = coords[i] * spec.Scale;
            variables["alpha"] = originColor.A / 255.0;

            var offset = spec.SpaceToken ? 1 : 0;
            var values = new double[3];
            for (var i = 0; i < 3; i++)
            {
                var token = comps[offset + i];
                var value = spec.IsHue(i) ? Relative.EvaluateHue(token, variables) : Relative.Evaluate(token, spec.References[i], variables);
                if (value is null) return null;
                values[i] = value.Value;
            }

            float alpha;
            if (alphaTok != null)
            {
                var a = Relative.Evaluate(alphaTok, 1.0, variables);
                if (a is null) return null;
                alpha = (float)Clamp(a.Value, 0, 1);
            }
            else
            {
                alpha = originColor.A / 255f;
            }

            return Relative.Build(spec, values, alpha, wantOklab, out oklab);
        }

        // What each function says about its components: the space its result is in, the channel keywords, what 100% of each component is, which one is a hue.
        private sealed class RelativeSpec
        {
            public RelativeSpec(ColorSpaceMath.Space space, string[] keywords, double[] references, int hueSlot, double scale, bool spaceToken, int componentCount, string function)
            {
                Space = space;
                Keywords = keywords;
                References = references;
                HueSlot = hueSlot;
                Scale = scale;
                SpaceToken = spaceToken;
                ComponentCount = componentCount;
                Function = function;
            }

            public ColorSpaceMath.Space Space { get; }
            public string[] Keywords { get; }
            public double[] References { get; }
            public int HueSlot { get; }
            public double Scale { get; }
            public bool SpaceToken { get; }
            public int ComponentCount { get; }
            public string Function { get; }

            public bool IsHue(int slot) => slot == HueSlot;
        }

        private static class Relative
        {
            public static bool TryGetSpec(string name, out RelativeSpec spec)
            {
                spec = null;
                switch (name)
                {
                    case "rgb":
                    case "rgba":
                        spec = new RelativeSpec(ColorSpaceMath.Space.Srgb, new[] { "r", "g", "b" }, new double[] { 255, 255, 255 }, -1, 255, false, 3, "rgb");
                        return true;
                    case "hsl":
                    case "hsla":
                        spec = new RelativeSpec(ColorSpaceMath.Space.Hsl, new[] { "h", "s", "l" }, new double[] { 360, 100, 100 }, 0, 1, false, 3, "hsl");
                        return true;
                    case "hwb":
                        spec = new RelativeSpec(ColorSpaceMath.Space.Hwb, new[] { "h", "w", "b" }, new double[] { 360, 100, 100 }, 0, 1, false, 3, "hwb");
                        return true;
                    case "lab":
                        spec = new RelativeSpec(ColorSpaceMath.Space.Lab, new[] { "l", "a", "b" }, new double[] { 100, 125, 125 }, -1, 1, false, 3, "lab");
                        return true;
                    case "oklab":
                        spec = new RelativeSpec(ColorSpaceMath.Space.Oklab, new[] { "l", "a", "b" }, new[] { 1, 0.4, 0.4 }, -1, 1, false, 3, "oklab");
                        return true;
                    case "lch":
                        spec = new RelativeSpec(ColorSpaceMath.Space.Lch, new[] { "l", "c", "h" }, new double[] { 100, 150, 360 }, 2, 1, false, 3, "lch");
                        return true;
                    case "oklch":
                        spec = new RelativeSpec(ColorSpaceMath.Space.Oklch, new[] { "l", "c", "h" }, new[] { 1, 0.4, 360 }, 2, 1, false, 3, "oklch");
                        return true;
                    case "color":
                        // The space is the first component; its keywords follow from it (TryReadSpace).
                        spec = new RelativeSpec(ColorSpaceMath.Space.Srgb, new[] { "r", "g", "b" }, new double[] { 1, 1, 1 }, -1, 1, true, 4, "color");
                        return true;
                    default:
                        return false;
                }
            }

            public static bool TryReadSpace(Token token, out RelativeSpec spec)
            {
                spec = null;
                if (token.Type != TokenType.Ident || !PredefinedSpaces.TryGetValue(token.Data, out var space)) return false;

                var isXyz = space == ColorSpaceMath.Space.XyzD65 || space == ColorSpaceMath.Space.XyzD50;
                spec = new RelativeSpec(space, isXyz ? new[] { "x", "y", "z" } : new[] { "r", "g", "b" }, new double[] { 1, 1, 1 }, -1, 1, true, 4, "color");
                return true;
            }

            // The origin's components in the space of the function it is relative to. A plain sRGB color converts from its own 8-bit value (so rgb(from red r g b) is red
            // exactly); one that can lie outside the gamut from the coordinates it was written with, unclipped.
            public static double[] OriginCoordinates(Color origin, double[] oklab, ColorSpaceMath.Space space)
            {
                if (oklab != null && space != ColorSpaceMath.Space.Hsl && space != ColorSpaceMath.Space.Hwb)
                    return ColorSpaceMath.ToSpaceFromOklab(oklab, space);

                return ColorSpaceMath.ToSpace(origin.R / 255.0, origin.G / 255.0, origin.B / 255.0, space);
            }

            // A component: a number (or a percentage of the reference), none (0), a channel keyword, or a calc() over the keywords.
            public static double? Evaluate(Token token, double reference, Dictionary<string, double> variables)
            {
                if (IsNone(token)) return 0.0;

                if (token.Type == TokenType.Ident)
                    return variables.TryGetValue(token.Data.ToLowerInvariant(), out var keyword) ? keyword : (double?)null;

                if (token is FunctionToken function && CalcParser.IsCalcFamily(function.Data))
                {
                    var result = Calc(CalcParser.Parse(function, variables));
                    if (result is null) return null;
                    switch (result.Value.Category)
                    {
                        case CalcCategory.Number: return result.Value.Value;
                        case CalcCategory.Percentage: return result.Value.Value / 100.0 * reference;
                        default: return null;
                    }
                }

                return Component(token, reference);
            }

            public static double? EvaluateHue(Token token, Dictionary<string, double> variables)
            {
                if (IsNone(token)) return 0.0;

                if (token.Type == TokenType.Ident)
                    return variables.TryGetValue(token.Data.ToLowerInvariant(), out var keyword) ? keyword : (double?)null;

                if (token is FunctionToken function && CalcParser.IsCalcFamily(function.Data))
                {
                    var result = Calc(CalcParser.Parse(function, variables));
                    if (result is null) return null;
                    return result.Value.Category == CalcCategory.Number || result.Value.Category == CalcCategory.Angle ? result.Value.Value : (double?)null;
                }

                return HueDegrees(token);
            }

            // A calc() tree to a number, with the category it type-checks as: numbers, percentages and angles (in degrees), the only kinds a color component takes.
            // Anything else (lengths, times, ...) is not a color component, so it is null.
            private static (double Value, CalcCategory Category)? Calc(CalcNode node)
            {
                switch (node)
                {
                    case NumberCalcNode number:
                        return (number.Value, CalcCategory.Number);
                    case PercentageCalcNode percentage:
                        return (percentage.Value, CalcCategory.Percentage);
                    case AngleCalcNode angle:
                        return (ToDegrees(angle), CalcCategory.Angle);
                    case UnaryCalcNode unary:
                    {
                        var operand = Calc(unary.Operand);
                        if (operand is null) return null;
                        return (unary.Negative ? -operand.Value.Value : operand.Value.Value, operand.Value.Category);
                    }
                    case BinaryCalcNode binary:
                    {
                        var left = Calc(binary.Left);
                        var right = Calc(binary.Right);
                        if (left is null || right is null) return null;
                        var l = left.Value;
                        var r = right.Value;
                        switch (binary.Operator)
                        {
                            case '+' when l.Category == r.Category:
                                return (l.Value + r.Value, l.Category);
                            case '-' when l.Category == r.Category:
                                return (l.Value - r.Value, l.Category);
                            case '*' when l.Category == CalcCategory.Number:
                                return (l.Value * r.Value, r.Category);
                            case '*' when r.Category == CalcCategory.Number:
                                return (l.Value * r.Value, l.Category);
                            case '/' when r.Category == CalcCategory.Number && r.Value != 0:
                                return (l.Value / r.Value, l.Category);
                            default:
                                return null;
                        }
                    }
                    case CallCalcNode call:
                    {
                        var values = new List<(double Value, CalcCategory Category)>(call.Arguments.Count);
                        foreach (var argument in call.Arguments)
                        {
                            var value = Calc(argument);
                            if (value is null) return null;
                            if (values.Count > 0 && values[0].Category != value.Value.Category) return null;
                            values.Add(value.Value);
                        }

                        if (values.Count == 0) return null;
                        var category = values[0].Category;
                        if (call.Name.Equals(FunctionNames.Min, StringComparison.OrdinalIgnoreCase)) return (values.Min(v => v.Value), category);
                        if (call.Name.Equals(FunctionNames.Max, StringComparison.OrdinalIgnoreCase)) return (values.Max(v => v.Value), category);
                        if (call.Name.Equals(FunctionNames.Clamp, StringComparison.OrdinalIgnoreCase) && values.Count == 3)
                            return (values[0].Value > values[2].Value ? values[2].Value : Clamp(values[1].Value, values[0].Value, values[2].Value), category);
                        return null;
                    }
                    default:
                        return null;
                }
            }

            private static double ToDegrees(AngleCalcNode angle)
            {
                switch (angle.Unit)
                {
                    case Angle.Unit.Grad: return angle.Value * 0.9;
                    case Angle.Unit.Turn: return angle.Value * 360.0;
                    case Angle.Unit.Rad: return angle.Value * 180.0 / Math.PI;
                    default: return angle.Value;
                }
            }

            public static Color? Build(RelativeSpec spec, double[] v, float alpha, bool wantOklab, out double[] oklab)
            {
                oklab = null;
                switch (spec.Function)
                {
                    case "rgb":
                        return new Color((byte)Math.Round(Clamp(v[0], 0, 255)), (byte)Math.Round(Clamp(v[1], 0, 255)), (byte)Math.Round(Clamp(v[2], 0, 255)), Clamp01Byte(alpha));
                    case "hsl":
                        return Color.FromHsla((float)(v[0] / 360.0), (float)(v[1] / 100.0), (float)(v[2] / 100.0), alpha);
                    case "hwb":
                        return Color.FromHwba((float)(v[0] / 360.0), (float)(v[1] / 100.0), (float)(v[2] / 100.0), alpha);
                    case "lab":
                    case "oklab":
                    {
                        var coords = new[] { Clamp(v[0], 0, spec.References[0]), v[1], v[2] };
                        if (wantOklab) oklab = ColorSpaceMath.ToOklab(coords, spec.Space);
                        return Color.FromSpaceComponents(coords, spec.Space, alpha);
                    }
                    case "lch":
                    case "oklch":
                    {
                        var coords = new[] { Clamp(v[0], 0, spec.References[0]), Math.Max(v[1], 0), v[2] };
                        if (wantOklab) oklab = ColorSpaceMath.ToOklab(coords, spec.Space);
                        return Color.FromSpaceComponents(coords, spec.Space, alpha);
                    }
                    default:
                        if (wantOklab) oklab = ColorSpaceMath.ToOklab(v, spec.Space);
                        return Color.FromSpaceComponents(v, spec.Space, alpha);
                }
            }
        }
    }
}
