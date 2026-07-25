using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Implements CSS calc()'s type-checking rules over a parsed <see cref="CalcNode"/> tree (Layer A
    /// only — Layer B's evaluator trusts that a value it's asked to evaluate already passed this check,
    /// see CalcValueConverter/CalcEvaluator). Also folds pure-Number subtrees to concrete values, which
    /// is what makes a constant divide-by-zero divisor detectable here rather than deferred to layout.
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
                {
                    var left = Check(binary.Left);
                    var right = Check(binary.Right);
                    return Combine(left, right);
                }

                case BinaryCalcNode binary when binary.Operator == '*':
                {
                    var left = Check(binary.Left);
                    var right = Check(binary.Right);
                    if (left is null || right is null) return null;
                    if (left == CalcCategory.Number) return right;
                    if (right == CalcCategory.Number) return left;
                    return null;
                }

                case BinaryCalcNode binary when binary.Operator == '/':
                {
                    var left = Check(binary.Left);
                    var right = Check(binary.Right);
                    if (left is null || right != CalcCategory.Number) return null;

                    // Every valid Number-category subtree is built entirely from Number leaves, so it's
                    // always foldable here — this is what lets a constant divide-by-zero be rejected at
                    // validation time rather than silently producing an infinity/NaN at layout time.
                    var divisor = FoldNumber(binary.Right);
                    return divisor == null || divisor == 0d ? null : left;
                }

                case CallCalcNode call:
                {
                    CalcCategory? combined = null;
                    foreach (var argument in call.Arguments)
                    {
                        var category = Check(argument);
                        if (category is null) return null;
                        combined = combined is null ? category : Combine(combined, category);
                        if (combined is null) return null;
                    }

                    return combined;
                }

                default:
                    return null;
            }
        }

        /// <summary>Folds a pure-Number subtree to a concrete value; null if it isn't purely numeric.</summary>
        public static double? FoldNumber(CalcNode node)
        {
            switch (node)
            {
                case NumberCalcNode number:
                    return number.Value;

                case UnaryCalcNode unary:
                {
                    var value = FoldNumber(unary.Operand);
                    return value is null ? null : (double?)(unary.Negative ? -value.Value : value.Value);
                }

                case BinaryCalcNode binary:
                {
                    var left = FoldNumber(binary.Left);
                    var right = FoldNumber(binary.Right);
                    if (left is null || right is null) return null;

                    switch (binary.Operator)
                    {
                        case '+':
                            return left.Value + right.Value;
                        case '-':
                            return left.Value - right.Value;
                        case '*':
                            return left.Value * right.Value;
                        case '/':
                            return right.Value != 0d ? (double?)(left.Value / right.Value) : null;
                        default:
                            return null;
                    }
                }

                case CallCalcNode call:
                {
                    var values = new List<double>(call.Arguments.Count);
                    foreach (var argument in call.Arguments)
                    {
                        var value = FoldNumber(argument);
                        if (value is null) return null;
                        values.Add(value.Value);
                    }

                    if (call.Name.Isi(FunctionNames.Min)) return values.Min();
                    if (call.Name.Isi(FunctionNames.Max)) return values.Max();
                    if (call.Name.Isi(FunctionNames.Clamp) && values.Count == 3)
                        return values[0] > values[2] ? values[2] : Math.Max(values[0], Math.Min(values[2], values[1]));

                    return null;
                }

                default:
                    // Dimension/Percentage leaves are never purely numeric.
                    return null;
            }
        }

        private static CalcCategory? Combine(CalcCategory? left, CalcCategory? right)
        {
            if (left is null || right is null) return null;
            if (left == CalcCategory.Number && right == CalcCategory.Number) return CalcCategory.Number;
            if (left.Value.HasFlag(CalcCategory.Number) || right.Value.HasFlag(CalcCategory.Number)) return null;
            return left.Value | right.Value;
        }
    }
}
