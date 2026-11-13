using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A parsed grid <c>&lt;grid-line&gt;</c> (one edge of <c>grid-column</c>/<c>grid-row</c>/<c>grid-area</c>):
    /// <c>auto</c>, an integer line number (may be negative — counting from the end edge), or <c>span N</c>.
    /// Named lines (<c>&lt;custom-ident&gt;</c>) are populated by the named-line grammar extension.
    /// </summary>
    internal sealed class GridLine
    {
        public bool IsAuto { get; private set; }
        public bool IsSpan { get; private set; }

        /// <summary>The line number (for a line reference), the span count (when <see cref="IsSpan"/>), or
        /// the 1-based Nth index for a named line (defaults to 1).</summary>
        public int Value { get; private set; } = 1;

        /// <summary>The custom-ident of a named line reference, or null when this is not a named line.</summary>
        public string Name { get; private set; }

        public static readonly GridLine Auto = new() { IsAuto = true };
        public static GridLine Span(int n) => new() { IsSpan = true, Value = n };
        public static GridLine Line(int n) => new() { Value = n };
        public static GridLine Named(string name) => new() { Name = name, Value = 1 };
        public static GridLine NamedNth(string name, int n) => new() { Name = name, Value = n };
    }

    /// <summary>
    /// Shared grammar for a single grid <c>&lt;grid-line&gt;</c> value. Used to validate the
    /// <c>grid-column-start</c>/<c>-end</c>/<c>grid-row-start</c>/<c>-end</c> longhands and the placement
    /// shorthands.
    /// </summary>
    internal static class GridLineGrammar
    {
        internal static GridLine TryParse(IReadOnlyList<Token> tokens)
        {
            var toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (toks.Length == 0) return null;

            // auto
            if (toks.Length == 1 && toks[0].Type == TokenType.Ident && toks[0].Data.Isi(Keywords.Auto))
                return GridLine.Auto;

            // <integer>
            if (toks.Length == 1 && toks[0] is NumberToken { IsInteger: true } number && number.IntegerValue != 0)
                return GridLine.Line(number.IntegerValue);

            // span <integer [1,∞]>
            if (toks.Length == 2 && toks[0].Type == TokenType.Ident && toks[0].Data.Isi(Keywords.Span)
                && toks[1] is NumberToken { IsInteger: true } spanCount && spanCount.IntegerValue >= 1)
                return GridLine.Span(spanCount.IntegerValue);

            // <custom-ident> — a named line reference.
            if (toks.Length == 1 && IsCustomIdent(toks[0]))
                return GridLine.Named(toks[0].Data);

            // <custom-ident> <integer> / <integer> <custom-ident> — the Nth line with that name (order-independent).
            if (toks.Length == 2)
            {
                if (IsCustomIdent(toks[0]) && toks[1] is NumberToken { IsInteger: true } n1 && n1.IntegerValue != 0)
                    return GridLine.NamedNth(toks[0].Data, n1.IntegerValue);
                if (IsCustomIdent(toks[1]) && toks[0] is NumberToken { IsInteger: true } n2 && n2.IntegerValue != 0)
                    return GridLine.NamedNth(toks[1].Data, n2.IntegerValue);
            }

            return null;
        }

        /// <summary>Whether the token is a <c>&lt;custom-ident&gt;</c> usable as a grid line name — an ident
        /// that is not one of the reserved keywords a grid-line value or the CSS-wide keywords may take.</summary>
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
