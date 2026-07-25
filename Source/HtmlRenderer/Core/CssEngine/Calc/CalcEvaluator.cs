using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The layout-time context needed to resolve relative units within a calc-family expression: how
    /// many pixels 1em/1rem are, and what 100% means for this particular call site (e.g. containing-block
    /// width for <c>width</c>, parent font-size for <c>font-size</c>'s em-relative resolution).
    /// </summary>
    internal readonly struct CalcContext
    {
        public CalcContext(double hundredPercent, double emFactor, double remFactor, bool fontAdjust, bool returnPoints = false)
        {
            HundredPercent = hundredPercent;
            EmFactor = emFactor;
            RemFactor = remFactor;
            FontAdjust = fontAdjust;
            ReturnPoints = returnPoints;
        }

        public double HundredPercent { get; }
        public double EmFactor { get; }
        public double RemFactor { get; }
        public bool FontAdjust { get; }

        /// <summary>
        /// Mirrors ParseLength's returnPoints parameter: when the caller's em/rem/percent factors are
        /// already expressed in points (as CssBoxProperties.FontSize's caller does), a bare <c>pt</c> leaf
        /// must bypass the normal pt-&gt;px factor rather than being converted into this already-points space.
        /// </summary>
        public bool ReturnPoints { get; }
    }

    /// <summary>
    /// Evaluates a validated calc-family AST to a pixel-space number. This is the one place calc()
    /// numbers actually get computed — called only from Layer B (<c>CssValueParser.ParseLength</c>),
    /// since only layout has the <see cref="CalcContext"/> a percentage/em/rem leaf needs to resolve.
    /// Reuses <see cref="Length.ToPixels"/> for every leaf, so no unit-conversion arithmetic is duplicated
    /// here. A null result signals a divide-by-zero; per the type-checker's rules every legal divisor is
    /// a constant that Layer A already folds and rejects at validation time, so in practice this is a
    /// defensive fallback rather than a load-bearing check.
    /// </summary>
    internal static class CalcEvaluator
    {
        public static double? Evaluate(CalcNode node, CalcContext context)
        {
            switch (node)
            {
                case NumberCalcNode number:
                    return number.Value;

                case DimensionCalcNode dimension when dimension.Unit == Length.Unit.Pt && context.ReturnPoints:
                    return dimension.Value;

                case DimensionCalcNode dimension:
                    return new Length((float)dimension.Value, dimension.Unit)
                        .ToPixels(context.EmFactor, context.RemFactor, context.HundredPercent, context.FontAdjust);

                case PercentageCalcNode percentage:
                    return new Length((float)percentage.Value, Length.Unit.Percent)
                        .ToPixels(context.EmFactor, context.RemFactor, context.HundredPercent, context.FontAdjust);

                case AngleCalcNode angle:
                    // An angle leaf evaluates to radians (the canonical angle unit), so an angle-typed
                    // calc such as `calc(1turn * 0.35)` yields 0.35 turn in radians (a Number leaf stays a
                    // unitless scalar). Length calc()s never contain an angle leaf, so this never perturbs
                    // length evaluation; the only current caller is conic-gradient stop-position parsing.
                    return new Angle((float)angle.Value, angle.Unit).ToRadian();

                case TimeCalcNode time:
                    // Canonical time unit is milliseconds. No layout property evaluates a time calc() today
                    // (these nodes only arise from @property `syntax: "<time>"` validation, which type-checks
                    // rather than evaluates), so this case is defensive completeness paralleling the angle leaf.
                    return new Time((float)time.Value, time.Unit).ToMilliseconds();

                case ResolutionCalcNode resolution:
                    // Canonical resolution unit is dots-per-pixel; same @property-only provenance as TimeCalcNode.
                    return new Resolution((float)resolution.Value, resolution.Unit).ToDotsPerPixel();

                case UnaryCalcNode unary:
                {
                    var operand = Evaluate(unary.Operand, context);
                    if (operand is null) return null;
                    return unary.Negative ? -operand.Value : operand.Value;
                }

                case BinaryCalcNode binary:
                {
                    var left = Evaluate(binary.Left, context);
                    var right = Evaluate(binary.Right, context);
                    if (left is null || right is null) return null;

                    if (binary.Operator == '+') return left.Value + right.Value;
                    if (binary.Operator == '-') return left.Value - right.Value;
                    if (binary.Operator == '*') return left.Value * right.Value;
                    if (binary.Operator == '/') return right.Value != 0d ? (double?)(left.Value / right.Value) : null;
                    return null;
                }

                case CallCalcNode call:
                {
                    var values = new List<double>(call.Arguments.Count);
                    foreach (var argument in call.Arguments)
                    {
                        var value = Evaluate(argument, context);
                        if (value is null) return null;
                        values.Add(value.Value);
                    }

                    if (call.Name.Isi(FunctionNames.Min)) return Min(values);
                    if (call.Name.Isi(FunctionNames.Max)) return Max(values);
                    if (call.Name.Isi(FunctionNames.Clamp) && values.Count == 3)
                        return values[0] > values[2] ? values[2] : Math.Min(Math.Max(values[1], values[0]), values[2]);

                    return null;
                }

                default:
                    return null;
            }
        }

        private static double Min(List<double> values)
        {
            var result = values[0];
            for (var i = 1; i < values.Count; i++) if (values[i] < result) result = values[i];
            return result;
        }

        private static double Max(List<double> values)
        {
            var result = values[0];
            for (var i = 1; i < values.Count; i++) if (values[i] > result) result = values[i];
            return result;
        }
    }
}
