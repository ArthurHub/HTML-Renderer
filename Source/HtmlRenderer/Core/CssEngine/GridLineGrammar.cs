using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>A <c>&lt;grid-line&gt;</c>, CSS Grid 2 §8.3.</summary>
    internal sealed class GridLine
    {
        public bool IsAuto { get; private set; }
        public bool IsSpan { get; private set; }

        /// <summary>The line number, the span count, or which line of the name it is.</summary>
        public int Value { get; private set; } = 1;

        public string Name { get; private set; }

        public static readonly GridLine Auto = new GridLine { IsAuto = true };
        public static GridLine Span(int n) => new GridLine { IsSpan = true, Value = n };
        public static GridLine Line(int n) => new GridLine { Value = n };
        public static GridLine Named(string name) => new GridLine { Name = name, Value = 1 };
        public static GridLine NamedNth(string name, int n) => new GridLine { Name = name, Value = n };
    }

    /// <summary>The <c>&lt;grid-line&gt;</c> of the <c>grid-row-*</c>/<c>grid-column-*</c> longhands, CSS Grid 2 §8.3.</summary>
    internal static class GridLineGrammar
    {
        internal static GridLine TryParse(IReadOnlyList<Token> tokens)
        {
            var toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (toks.Length == 0) return null;

            if (toks.Length == 1 && toks[0].Type == TokenType.Ident && toks[0].Data.Isi(Keywords.Auto))
                return GridLine.Auto;

            if (toks.Length == 1 && toks[0] is NumberToken number && number.IsInteger && number.IntegerValue != 0)
                return GridLine.Line(number.IntegerValue);

            if (toks.Length == 2 && toks[0].Type == TokenType.Ident && toks[0].Data.Isi(Keywords.Span)
                && toks[1] is NumberToken spanCount && spanCount.IsInteger && spanCount.IntegerValue >= 1)
                return GridLine.Span(spanCount.IntegerValue);

            if (toks.Length == 1 && IsCustomIdent(toks[0]))
                return GridLine.Named(toks[0].Data);

            if (toks.Length == 2)
            {
                if (IsCustomIdent(toks[0]) && toks[1] is NumberToken n1 && n1.IsInteger && n1.IntegerValue != 0)
                    return GridLine.NamedNth(toks[0].Data, n1.IntegerValue);
                if (IsCustomIdent(toks[1]) && toks[0] is NumberToken n2 && n2.IsInteger && n2.IntegerValue != 0)
                    return GridLine.NamedNth(toks[1].Data, n2.IntegerValue);
            }

            return null;
        }

        private static bool IsCustomIdent(Token token)
        {
            if (token.Type != TokenType.Ident) return false;
            var d = token.Data;
            return !d.Isi(Keywords.Auto) && !d.Isi(Keywords.Span) && !d.Isi(Keywords.None)
                && !d.Isi(Keywords.Inherit) && !d.Isi(Keywords.Initial) && !d.Isi(Keywords.Unset)
                && !d.Isi(Keywords.Revert) && !d.Isi(Keywords.RevertLayer) && !d.Isi("default");
        }
    }
}
