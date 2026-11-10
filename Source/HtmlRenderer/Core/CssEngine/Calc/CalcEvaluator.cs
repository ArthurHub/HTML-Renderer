using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// What em, rem and 100% are in pixels where a calc() is resolved.
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
        /// The factors are in points instead of pixels, so a pt value is taken as is, like
        /// <see cref="Parse.CssValueParser.ParseLength(string, double, double, string, bool, bool)"/>.
        /// </summary>
        public bool ReturnPoints { get; }
    }

    /// <summary>
    /// Evaluates a type-checked calc() tree at layout time, lengths to pixels, angles to radians,
    /// times to milliseconds and resolutions to dppx. Null on a division by zero, which the type
    /// checker already rejects.
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
                    return new Angle((float)angle.Value, angle.Unit).ToRadian();

                case TimeCalcNode time:
                    return new Time((float)time.Value, time.Unit).ToMilliseconds();

                case ResolutionCalcNode resolution:
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
