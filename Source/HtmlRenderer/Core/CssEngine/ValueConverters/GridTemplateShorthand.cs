using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>Parsing shared by <c>grid-template</c> (CSS Grid 2 §7.4) and <c>grid</c> (§7.8).</summary>
    internal static class GridTemplateShorthand
    {
        internal static readonly Token AutoToken = new Token(TokenType.Ident, Keywords.Auto, TextPosition.Empty);
        private static readonly Token NoneToken = new Token(TokenType.Ident, Keywords.None, TextPosition.Empty);

        /// <summary>
        /// No nesting depth needed, a function is a single <see cref="FunctionToken"/> and <c>[name]</c>
        /// groups only hold idents.
        /// </summary>
        internal static List<List<Token>> SplitTopLevelSlash(IEnumerable<Token> value)
        {
            var groups = new List<List<Token>> { new List<Token>() };

            foreach (var token in value)
            {
                if (token.Type == TokenType.Delim && token.Data == "/") groups.Add(new List<Token>());
                else groups[groups.Count - 1].Add(token);
            }

            return groups;
        }

        internal static Token[] Significant(IEnumerable<Token> tokens) =>
            tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

        /// <summary>One side of <c>&lt;rows&gt; / &lt;columns&gt;</c>, trimmed, or null if invalid.</summary>
        internal static IReadOnlyList<Token> TryTemplateAxis(IReadOnlyList<Token> raw)
        {
            var significant = Significant(raw);
            if (significant.Length == 0) return null;
            if (significant.Length == 1 && significant[0].Type == TokenType.Ident && significant[0].Data.Isi(Keywords.None)) return Trim(raw);
            return GridTrackListGrammar.TryParse(raw) != null ? Trim(raw) : null;
        }

        private static bool ContainsRepeat(IEnumerable<Token> significant) =>
            significant.Any(t => t is FunctionToken fn && fn.Data.Isi(FunctionNames.Repeat));

        internal static IReadOnlyList<Token> Trim(IReadOnlyList<Token> tokens)
        {
            var start = 0;
            var end = tokens.Count - 1;
            while (start <= end && tokens[start].Type == TokenType.Whitespace) start++;
            while (end >= start && tokens[end].Type == TokenType.Whitespace) end--;

            var result = new List<Token>(end - start + 1);
            for (var i = start; i <= end; i++) result.Add(tokens[i]);
            return result;
        }

        internal static IReadOnlyList<Token> BuildAutoFlow(bool column, bool dense)
        {
            var flow = new List<Token> { new Token(TokenType.Ident, column ? Keywords.Column : Keywords.Row, TextPosition.Empty) };
            if (dense)
            {
                flow.Add(Token.Whitespace);
                flow.Add(new Token(TokenType.Ident, Keywords.Dense, TextPosition.Empty));
            }
            return flow;
        }

        /// <summary>A null slice means the longhand is reset to its initial value.</summary>
        internal static bool TryParseTemplate(IEnumerable<Token> value,
            out IReadOnlyList<Token> rows, out IReadOnlyList<Token> columns, out IReadOnlyList<Token> areas)
        {
            rows = columns = areas = null;

            var groups = SplitTopLevelSlash(value);
            if (groups.Count > 2) return false;

            var leftRaw = groups[0];
            var rightRaw = groups.Count == 2 ? groups[1] : null;
            var left = Significant(leftRaw);
            var right = rightRaw is null ? null : Significant(rightRaw);

            if (right == null && left.Length == 1 && left[0].Type == TokenType.Ident && left[0].Data.Isi(Keywords.None))
            {
                rows = columns = areas = new[] { NoneToken };
                return true;
            }

            if (left.Length == 0) return false;

            if (left.Any(t => t.Type == TokenType.String))
            {
                if (!TryScanAreasForm(left, out var rowsTokens, out var areaTokens)) return false;
                if (GridTemplateAreasGrammar.TryParse(areaTokens) is null) return false;
                if (GridTrackListGrammar.TryParse(rowsTokens) is null) return false;

                rows = rowsTokens;
                areas = areaTokens;

                if (rightRaw != null)
                {
                    // <explicit-track-list>, no repeat()
                    if (right.Length == 0 || ContainsRepeat(right) || GridTrackListGrammar.TryParse(rightRaw) is null)
                        return false;
                    columns = Trim(rightRaw);
                }

                return true;
            }

            if (rightRaw is null) return false;
            var rowsSlice = TryTemplateAxis(leftRaw);
            var columnsSlice = TryTemplateAxis(rightRaw);
            if (rowsSlice is null || columnsSlice is null) return false;

            rows = rowsSlice;
            columns = columnsSlice;
            return true;
        }

        /// <summary>
        /// Splits the areas form into the <c>grid-template-rows</c> track list and the area strings, a row
        /// without a track size gets <c>auto</c>.
        /// </summary>
        private static bool TryScanAreasForm(Token[] toks, out List<Token> rowsOut, out List<Token> areaStrings)
        {
            rowsOut = new List<Token>();
            areaStrings = new List<Token>();
            var sawString = false;

            var i = 0;
            while (i < toks.Length)
            {
                var t = toks[i];

                if (t.Type == TokenType.SquareBracketOpen)
                {
                    if (rowsOut.Count > 0) rowsOut.Add(Token.Whitespace);
                    rowsOut.Add(t);
                    i++;
                    var firstName = true;
                    while (i < toks.Length && toks[i].Type != TokenType.SquareBracketClose)
                    {
                        if (toks[i].Type != TokenType.Ident) return false;
                        if (!firstName) rowsOut.Add(Token.Whitespace);
                        rowsOut.Add(toks[i]);
                        firstName = false;
                        i++;
                    }
                    if (i >= toks.Length) return false;
                    rowsOut.Add(toks[i]);
                    i++;
                    continue;
                }

                if (t.Type == TokenType.String)
                {
                    Emit(areaStrings, t);
                    sawString = true;
                    i++;

                    if (i < toks.Length && toks[i].Type != TokenType.SquareBracketOpen && toks[i].Type != TokenType.String)
                    {
                        if (GridTrackListGrammar.TryParseTrackSizeList(new[] { toks[i] }) is null) return false;
                        Emit(rowsOut, toks[i]);
                        i++;
                    }
                    else
                    {
                        Emit(rowsOut, AutoToken);
                    }

                    continue;
                }

                return false;
            }

            return sawString;
        }

        private static void Emit(List<Token> tokens, Token token)
        {
            if (tokens.Count > 0) tokens.Add(Token.Whitespace);
            tokens.Add(token);
        }

        /// <summary>
        /// The <c>[ auto-flow &amp;&amp; dense? ] &lt;track-size&gt;*</c> side of <c>grid</c>, a null
        /// <paramref name="autoTracks"/> means it's reset to its initial value.
        /// </summary>
        internal static bool TryParseAutoFlowSide(IReadOnlyList<Token> raw, out bool dense, out IReadOnlyList<Token> autoTracks)
        {
            dense = false;
            autoTracks = null;
            var hasAutoFlow = false;

            var i = 0;
            while (i < raw.Count)
            {
                var t = raw[i];
                if (t.Type == TokenType.Whitespace) { i++; continue; }
                if (t.Type == TokenType.Ident && t.Data.Isi(Keywords.AutoFlow))
                {
                    if (hasAutoFlow) return false;
                    hasAutoFlow = true;
                    i++;
                    continue;
                }
                if (t.Type == TokenType.Ident && t.Data.Isi(Keywords.Dense))
                {
                    if (dense) return false;
                    dense = true;
                    i++;
                    continue;
                }
                break;
            }

            if (!hasAutoFlow) return false;

            var rest = new List<Token>();
            for (; i < raw.Count; i++) rest.Add(raw[i]);
            var trimmed = Trim(rest);

            if (trimmed.Count > 0)
            {
                if (GridTrackListGrammar.TryParseTrackSizeList(trimmed) is null) return false;
                autoTracks = trimmed;
            }

            return true;
        }
    }
}
