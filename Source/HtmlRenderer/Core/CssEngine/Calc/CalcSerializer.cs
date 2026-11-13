using System;
using System.Collections.Generic;
using System.Globalization;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Produces the canonical CSS text for a validated calc()/min()/max()/clamp() tree. An expression made
    /// only of numbers folds to a plain number; one made only of angles folds to a plain angle in
    /// <c>deg</c>; one made only of absolute lengths (px, cm, mm, Q, in, pt, pc) folds to a plain
    /// <c>px</c> length. Anything that needs layout context (em, rem, %, viewport units, ...) is
    /// re-serialized to normalized calc()/min()/max()/clamp() text, with spaces around binary operators
    /// and only the parentheses that precedence requires.
    /// </summary>
    internal static class CalcSerializer
    {
        public static string Serialize(CalcNode node, CalcCategory category)
        {
            if (category == CalcCategory.Number)
            {
                var number = CalcTypeChecker.FoldNumber(node);
                if (number != null) return FormatNumber(number.Value);
            }
            else if (category == CalcCategory.Angle)
            {
                // Every angle unit converts by a fixed constant, so a valid angle expression always folds.
                var degrees = FoldAngle(node);
                if (degrees != null) return new Angle((float) degrees.Value, Angle.Unit.Deg).ToString(null, CultureInfo.InvariantCulture);
            }
            else if (category == CalcCategory.Length && TryFoldPixels(node, out var pixels))
            {
                return new Length((float) pixels, Length.Unit.Px).ToString(null, CultureInfo.InvariantCulture);
            }

            return SerializeExpression(node);
        }

        /// <summary>
        /// Evaluates a pure-number expression and returns the folded value, or null when the tree is not
        /// purely numeric. This is the constant-folding used for <c>&lt;integer&gt;</c>/<c>&lt;number&gt;</c>
        /// contexts; it never needs layout context.
        /// </summary>
        public static double? EvaluateNumber(CalcNode node)
        {
            return CalcTypeChecker.FoldNumber(node);
        }

        private static string FormatNumber(double value)
        {
            return ((float) value).ToString(null, CultureInfo.InvariantCulture);
        }

        // ---- Angle folding (degrees) ----

        private static double? FoldAngle(CalcNode node)
        {
            switch (node)
            {
                case AngleCalcNode angle:
                    return ToDegrees(angle.Value, angle.Unit);

                case UnaryCalcNode unary:
                {
                    var value = FoldAngle(unary.Operand);
                    return value == null ? (double?) null : unary.Negative ? -value.Value : value.Value;
                }

                case BinaryCalcNode binary when binary.Operator == '+' || binary.Operator == '-':
                {
                    var left = FoldAngle(binary.Left);
                    var right = FoldAngle(binary.Right);
                    if (left == null || right == null) return null;
                    return binary.Operator == '+' ? left.Value + right.Value : left.Value - right.Value;
                }

                case BinaryCalcNode binary when binary.Operator == '*':
                {
                    var leftAngle = FoldAngle(binary.Left);
                    if (leftAngle != null)
                    {
                        var scalar = CalcTypeChecker.FoldNumber(binary.Right);
                        return scalar == null ? (double?) null : leftAngle.Value * scalar.Value;
                    }

                    var rightAngle = FoldAngle(binary.Right);
                    if (rightAngle == null) return null;
                    var factor = CalcTypeChecker.FoldNumber(binary.Left);
                    return factor == null ? (double?) null : rightAngle.Value * factor.Value;
                }

                case BinaryCalcNode binary when binary.Operator == '/':
                {
                    var left = FoldAngle(binary.Left);
                    var divisor = CalcTypeChecker.FoldNumber(binary.Right);
                    if (left == null || divisor == null || divisor.Value == 0d) return null;
                    return left.Value / divisor.Value;
                }

                case CallCalcNode call:
                    return FoldCall(call, FoldAngle);

                default:
                    return null;
            }
        }

        private static double ToDegrees(double value, Angle.Unit unit)
        {
            switch (unit)
            {
                case Angle.Unit.Grad: return value * 0.9d;
                case Angle.Unit.Rad: return value * (180d / Math.PI);
                case Angle.Unit.Turn: return value * 360d;
                default: return value;
            }
        }

        // ---- Absolute length folding (CSS pixels) ----

        private static bool TryFoldPixels(CalcNode node, out double pixels)
        {
            var value = FoldPixels(node);
            pixels = value ?? 0d;
            return value != null;
        }

        private static double? FoldPixels(CalcNode node)
        {
            switch (node)
            {
                // A unitless number is only ever a scalar operand of '*' or '/' in a Length expression.
                case NumberCalcNode number:
                    return number.Value;

                case DimensionCalcNode dimension:
                {
                    var length = new Length((float) dimension.Value, dimension.Unit);
                    return length.IsAbsolute ? length.ToPixel() : (double?) null;
                }

                case UnaryCalcNode unary:
                {
                    var value = FoldPixels(unary.Operand);
                    return value == null ? (double?) null : unary.Negative ? -value.Value : value.Value;
                }

                case BinaryCalcNode binary:
                {
                    var left = FoldPixels(binary.Left);
                    var right = FoldPixels(binary.Right);
                    if (left == null || right == null) return null;

                    switch (binary.Operator)
                    {
                        case '+': return left.Value + right.Value;
                        case '-': return left.Value - right.Value;
                        case '*': return left.Value * right.Value;
                        case '/': return right.Value != 0d ? left.Value / right.Value : (double?) null;
                        default: return null;
                    }
                }

                case CallCalcNode call:
                    return FoldCall(call, FoldPixels);

                default:
                    return null;
            }
        }

        private static double? FoldCall(CallCalcNode call, Func<CalcNode, double?> fold)
        {
            var values = new List<double>(call.Arguments.Count);
            foreach (var argument in call.Arguments)
            {
                var value = fold(argument);
                if (value == null) return null;
                values.Add(value.Value);
            }

            if (call.Name.Isi(FunctionNames.Min))
            {
                var result = values[0];
                for (var i = 1; i < values.Count; i++) if (values[i] < result) result = values[i];
                return result;
            }

            if (call.Name.Isi(FunctionNames.Max))
            {
                var result = values[0];
                for (var i = 1; i < values.Count; i++) if (values[i] > result) result = values[i];
                return result;
            }

            if (call.Name.Isi(FunctionNames.Clamp) && values.Count == 3)
            {
                // clamp(min, val, max) == max(min, min(val, max)); if min > max, min wins.
                var upper = values[1] < values[2] ? values[1] : values[2];
                return upper < values[0] ? values[0] : upper;
            }

            return null;
        }

        // ---- Normalized expression text ----

        private static string SerializeExpression(CalcNode node)
        {
            if (node is CallCalcNode call)
            {
                var args = new List<string>(call.Arguments.Count);
                foreach (var argument in call.Arguments) args.Add(SerializeOperand(argument, false, 0));
                return call.Name + "(" + string.Join(", ", args) + ")";
            }

            return "calc(" + SerializeOperand(node, false, 0) + ")";
        }

        // parentPrecedence: 0 = none, 1 = sum-level + and -, 2 = product-level * and /, 3 = unary operand.
        // Parentheses are added whenever omitting them would change the meaning on re-parse: a
        // lower-precedence child under a higher-precedence parent, or a same-precedence child on the right
        // (the operators are left-associative).
        private static string SerializeOperand(CalcNode node, bool isRightOperand, int parentPrecedence)
        {
            switch (node)
            {
                case NumberCalcNode number:
                    return FormatNumber(number.Value);

                // Units are always written here, unlike Length.ToString which drops the unit of a zero
                // length - a unitless 0 inside calc() would be a number and break type checking.
                case DimensionCalcNode dimension:
                    return FormatNumber(dimension.Value) + new Length(0f, dimension.Unit).UnitString;

                case PercentageCalcNode percentage:
                    return FormatNumber(percentage.Value) + "%";

                case AngleCalcNode angle:
                    return FormatNumber(angle.Value) + new Angle(0f, angle.Unit).UnitString;

                case TimeCalcNode time:
                    return new Time((float) time.Value, time.Unit).ToString(null, CultureInfo.InvariantCulture);

                case ResolutionCalcNode resolution:
                    return new Resolution((float) resolution.Value, resolution.Unit)
                        .ToString(null, CultureInfo.InvariantCulture);

                case UnaryCalcNode unary:
                {
                    var operand = SerializeOperand(unary.Operand, false, 3);
                    if (!unary.Negative) return operand;

                    // "--x" would not re-parse as a double negation.
                    return operand.StartsWith("-", StringComparison.Ordinal) ? "-(" + operand + ")" : "-" + operand;
                }

                case CallCalcNode call:
                    return SerializeExpression(call);

                case BinaryCalcNode binary:
                {
                    var precedence = binary.Operator == '+' || binary.Operator == '-' ? 1 : 2;
                    var text = SerializeOperand(binary.Left, false, precedence) + " " + binary.Operator + " " +
                               SerializeOperand(binary.Right, true, precedence);
                    var needsParentheses = precedence < parentPrecedence ||
                                           (precedence == parentPrecedence && isRightOperand);
                    return needsParentheses ? "(" + text + ")" : text;
                }

                default:
                    return string.Empty;
            }
        }
    }
}
