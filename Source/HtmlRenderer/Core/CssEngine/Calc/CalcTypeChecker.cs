using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Implements CSS calc()'s type-checking rules (CSS Values 4 §10.10) over a parsed
    /// <see cref="CalcNode"/> tree, returning the resolved category or null when the expression is
    /// dimensionally inconsistent (e.g. adding a length to a time, or dividing by something that isn't a
    /// number). Also folds pure-number subtrees, which is what makes a constant divide-by-zero divisor
    /// detectable here.
    /// </summary>
    internal static class CalcTypeChecker
    {
        public static CalcCategory? Check(CalcNode node)
        {
            switch (node)
            {
                case NumberCalcNode _:
                    return CalcCategory.Number;

                case DimensionCalcNode _:
                    return CalcCategory.Length;

                case PercentageCalcNode _:
                    return CalcCategory.Percentage;

                case AngleCalcNode _:
                    return CalcCategory.Angle;

                case TimeCalcNode _:
                    return CalcCategory.Time;

                case ResolutionCalcNode _:
                    return CalcCategory.Resolution;

                case UnaryCalcNode unary:
                    return Check(unary.Operand);

                case BinaryCalcNode binary when binary.Operator == '+' || binary.Operator == '-':
                    return Combine(Check(binary.Left), Check(binary.Right));

                case BinaryCalcNode binary when binary.Operator == '*':
                {
                    var left = Check(binary.Left);
                    var right = Check(binary.Right);
                    if (left == null || right == null) return null;
                    if (left == CalcCategory.Number) return right;
                    if (right == CalcCategory.Number) return left;
                    return null;
                }

                case BinaryCalcNode binary when binary.Operator == '/':
                {
                    var left = Check(binary.Left);
                    var right = Check(binary.Right);
                    if (left == null || right != CalcCategory.Number) return null;

                    // A Number-category subtree is built entirely from number leaves, so it always folds -
                    // this is what lets a constant divide-by-zero be rejected here rather than deferred.
                    var divisor = FoldNumber(binary.Right);
                    return divisor == null || divisor.Value == 0d ? (CalcCategory?) null : left;
                }

                case CallCalcNode call:
                {
                    CalcCategory? combined = null;
                    foreach (var argument in call.Arguments)
                    {
                        var category = Check(argument);
                        if (category == null) return null;
                        combined = combined == null ? category : Combine(combined, category);
                        if (combined == null) return null;
                    }

                    return combined;
                }

                default:
                    return null;
            }
        }

        // Folds a pure-number subtree to a concrete value; null if it isn't purely numeric.
        public static double? FoldNumber(CalcNode node)
        {
            switch (node)
            {
                case NumberCalcNode number:
                    return number.Value;

                case UnaryCalcNode unary:
                {
                    var value = FoldNumber(unary.Operand);
                    return value == null ? (double?) null : unary.Negative ? -value.Value : value.Value;
                }

                case BinaryCalcNode binary:
                {
                    var left = FoldNumber(binary.Left);
                    var right = FoldNumber(binary.Right);
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
                {
                    var values = new List<double>(call.Arguments.Count);
                    foreach (var argument in call.Arguments)
                    {
                        var value = FoldNumber(argument);
                        if (value == null) return null;
                        values.Add(value.Value);
                    }

                    if (call.Name.Isi(FunctionNames.Min)) return values.Min();
                    if (call.Name.Isi(FunctionNames.Max)) return values.Max();
                    if (call.Name.Isi(FunctionNames.Clamp) && values.Count == 3)
                    {
                        var min = values[0];
                        var val = values[1];
                        var max = values[2];
                        // clamp(min, val, max) == max(min, min(val, max)); if min > max, min wins.
                        var upper = val < max ? val : max;
                        return upper < min ? min : upper;
                    }

                    return null;
                }

                default:
                    // Dimension/percentage leaves are never purely numeric.
                    return null;
            }
        }

        private static CalcCategory? Combine(CalcCategory? left, CalcCategory? right)
        {
            if (left == null || right == null) return null;
            if (left == CalcCategory.Number && right == CalcCategory.Number) return CalcCategory.Number;
            if (left.Value.HasFlag(CalcCategory.Number) || right.Value.HasFlag(CalcCategory.Number)) return null;
            return left.Value | right.Value;
        }
    }
}
