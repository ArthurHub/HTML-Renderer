using System.Collections.Generic;
using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ContainerCondition : IContainerCondition
    {
        private static readonly IReadOnlyList<IContainerCondition> NoOperands = new IContainerCondition[0];

        private ContainerCondition(ContainerConditionType type, IReadOnlyList<IContainerCondition> operands,
            string property, string value)
        {
            Type = type;
            Operands = operands;
            Property = property;
            Value = value;
        }

        public ContainerConditionType Type { get; }
        public IReadOnlyList<IContainerCondition> Operands { get; }
        public string Property { get; }
        public string Value { get; }

        public static ContainerCondition Size(string text) =>
            new ContainerCondition(ContainerConditionType.Size, NoOperands, null, text);

        public static ContainerCondition Declaration(string property, string value) =>
            new ContainerCondition(ContainerConditionType.Declaration, NoOperands, property, value);

        public static ContainerCondition Style(IContainerCondition query) =>
            new ContainerCondition(ContainerConditionType.Style, new[] { query }, null, null);

        public static ContainerCondition Not(IContainerCondition operand) =>
            new ContainerCondition(ContainerConditionType.Not, new[] { operand }, null, null);

        public static ContainerCondition And(List<IContainerCondition> operands) =>
            new ContainerCondition(ContainerConditionType.And, operands, null, null);

        public static ContainerCondition Or(List<IContainerCondition> operands) =>
            new ContainerCondition(ContainerConditionType.Or, operands, null, null);

        public void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            switch (Type)
            {
                case ContainerConditionType.Size:
                    writer.Write(Value);
                    break;
                case ContainerConditionType.Declaration:
                    writer.Write(Value == null ? Property : $"{Property}: {Value}");
                    break;
                case ContainerConditionType.Style:
                    // Directly inside style( ), a lone feature needs no parentheses of its own.
                    writer.Write("style(");
                    Operands[0].ToCss(writer, formatter);
                    writer.Write(")");
                    break;
                case ContainerConditionType.Not:
                    writer.Write("not ");
                    WriteOperand(Operands[0], writer, formatter);
                    break;
                default:
                    var connector = Type == ContainerConditionType.And ? " and " : " or ";
                    for (var i = 0; i < Operands.Count; i++)
                    {
                        if (i > 0) writer.Write(connector);
                        WriteOperand(Operands[i], writer, formatter);
                    }
                    break;
            }
        }

        // Features and nested combinators are parenthesised inside a combinator; style() and size features carry their own.
        private static void WriteOperand(IContainerCondition operand, TextWriter writer, IStyleFormatter formatter)
        {
            var wrap = operand.Type != ContainerConditionType.Size && operand.Type != ContainerConditionType.Style;
            if (wrap) writer.Write("(");
            operand.ToCss(writer, formatter);
            if (wrap) writer.Write(")");
        }
    }
}
