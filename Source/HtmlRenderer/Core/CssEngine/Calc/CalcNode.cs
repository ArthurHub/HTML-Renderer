using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// AST node for a parsed calc()/min()/max()/clamp() expression. Shared between Layer A (the CSS
    /// object model, which type-checks and canonicalizes it) and Layer B (layout, which evaluates it to
    /// a pixel number using context Layer A never has, such as containing-block width or font size).
    /// </summary>
    internal abstract class CalcNode
    {
    }

    internal sealed class NumberCalcNode : CalcNode
    {
        public NumberCalcNode(double value)
        {
            Value = value;
        }

        public double Value { get; }
    }

    internal sealed class DimensionCalcNode : CalcNode
    {
        public DimensionCalcNode(double value, Length.Unit unit)
        {
            Value = value;
            Unit = unit;
        }

        public double Value { get; }
        public Length.Unit Unit { get; }
    }

    internal sealed class PercentageCalcNode : CalcNode
    {
        public PercentageCalcNode(double value)
        {
            Value = value;
        }

        public double Value { get; }
    }

    internal sealed class AngleCalcNode : CalcNode
    {
        public AngleCalcNode(double value, Angle.Unit unit)
        {
            Value = value;
            Unit = unit;
        }

        public double Value { get; }
        public Angle.Unit Unit { get; }
    }

    /// <summary>A leading unary sign directly before a parenthesized group or a nested function call.</summary>
    internal sealed class UnaryCalcNode : CalcNode
    {
        public UnaryCalcNode(bool negative, CalcNode operand)
        {
            Negative = negative;
            Operand = operand;
        }

        public bool Negative { get; }
        public CalcNode Operand { get; }
    }

    /// <summary>A binary <c>+</c>/<c>-</c>/<c>*</c>/<c>/</c> operation.</summary>
    internal sealed class BinaryCalcNode : CalcNode
    {
        public BinaryCalcNode(char op, CalcNode left, CalcNode right)
        {
            Operator = op;
            Left = left;
            Right = right;
        }

        public char Operator { get; }
        public CalcNode Left { get; }
        public CalcNode Right { get; }
    }

    /// <summary>A <c>min()</c>, <c>max()</c>, or <c>clamp()</c> call over its comma-separated arguments.</summary>
    internal sealed class CallCalcNode : CalcNode
    {
        public CallCalcNode(string name, IReadOnlyList<CalcNode> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        public string Name { get; }
        public IReadOnlyList<CalcNode> Arguments { get; }
    }
}
