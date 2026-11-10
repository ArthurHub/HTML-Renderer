using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>CSS Grid 2 §7.2.1.</summary>
    internal enum GridTrackKind
    {
        Length,       // authored text in Value, a length or a length-percentage math function
        Percent,      // authored text in Value
        Flex,         // factor in Flex
        Auto,
        MinContent,
        MaxContent,
        FitContent,   // argument in Value
        Minmax        // Min, Max
    }

    /// <summary>CSS Grid 2 §7.2.3.2.</summary>
    internal enum GridAutoRepeatKind { None, AutoFill, AutoFit }

    /// <summary>A <c>&lt;track-size&gt;</c>, lengths and percentages keep their authored text.</summary>
    internal sealed class GridTrackSize
    {
        public GridTrackKind Kind { get; private set; }
        public string Value { get; private set; }
        public double Flex { get; private set; }
        public GridTrackSize Min { get; private set; }
        public GridTrackSize Max { get; private set; }

        public static GridTrackSize Length(string v) => new GridTrackSize { Kind = GridTrackKind.Length, Value = v };
        public static GridTrackSize Percent(string v) => new GridTrackSize { Kind = GridTrackKind.Percent, Value = v };
        public static GridTrackSize FlexFactor(double fr) => new GridTrackSize { Kind = GridTrackKind.Flex, Flex = fr };
        public static readonly GridTrackSize Auto = new GridTrackSize { Kind = GridTrackKind.Auto };
        public static readonly GridTrackSize MinContent = new GridTrackSize { Kind = GridTrackKind.MinContent };
        public static readonly GridTrackSize MaxContent = new GridTrackSize { Kind = GridTrackKind.MaxContent };
        public static GridTrackSize FitContentTo(string v) => new GridTrackSize { Kind = GridTrackKind.FitContent, Value = v };
        public static GridTrackSize Minmax(GridTrackSize min, GridTrackSize max) => new GridTrackSize { Kind = GridTrackKind.Minmax, Min = min, Max = max };
    }

    /// <summary>
    /// A <c>grid-template-columns</c>/<c>-rows</c> value. <c>repeat(&lt;integer&gt;, …)</c> is expanded into
    /// <see cref="Tracks"/>, an auto-fill/auto-fit repeat only depends on the container size and is kept apart.
    /// </summary>
    internal sealed class GridTemplate
    {
        /// <summary>The tracks without the auto-repeat ones, those go in at <see cref="AutoRepeatInsertIndex"/>.</summary>
        public IReadOnlyList<GridTrackSize> Tracks { get; set; } = new List<GridTrackSize>();

        public GridAutoRepeatKind AutoRepeat { get; set; } = GridAutoRepeatKind.None;

        public IReadOnlyList<GridTrackSize> AutoRepeatTracks { get; set; }

        public int AutoRepeatInsertIndex { get; set; }

        /// <summary>Line name → sorted 1-based line numbers, line 1 is before the first track.</summary>
        public IReadOnlyDictionary<string, IReadOnlyList<int>> LineNames { get; set; }
            = new Dictionary<string, IReadOnlyList<int>>();

        /// <summary>CSS Grid 2 §9, no <see cref="Tracks"/>, only the line names after the keyword.</summary>
        public bool IsSubgrid { get; set; }

        public bool IsNone => !IsSubgrid && Tracks.Count == 0 && AutoRepeat == GridAutoRepeatKind.None;
    }

    /// <summary>
    /// CSS Grid 2 §7.2 track lists of <c>grid-template-columns</c>/<c>-rows</c> and the
    /// <c>&lt;track-size&gt;+</c> of <c>grid-auto-columns</c>/<c>-rows</c>.
    /// </summary>
    internal static class GridTrackListGrammar
    {
        /// <summary>Returns null for <c>none</c> too, the property handles that keyword.</summary>
        internal static GridTemplate TryParse(IReadOnlyList<Token> tokens)
        {
            var toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (toks.Length == 0) return null;

            if (toks[0].Type == TokenType.Ident && toks[0].Data.Isi(Keywords.Subgrid))
                return TryParseSubgrid(toks);

            var fixedTracks = new List<GridTrackSize>();
            var autoRepeat = GridAutoRepeatKind.None;
            List<GridTrackSize> autoRepeatTracks = null;
            var autoRepeatIndex = -1;
            var lineNames = new Dictionary<string, SortedSet<int>>();

            var i = 0;
            while (i < toks.Length)
            {
                if (toks[i].Type == TokenType.SquareBracketOpen)
                {
                    var lineIndex = fixedTracks.Count + 1;
                    i++;
                    while (i < toks.Length && toks[i].Type != TokenType.SquareBracketClose)
                    {
                        if (toks[i].Type != TokenType.Ident) return null; // only idents inside [ ]
                        var name = toks[i].Data;
                        if (!lineNames.TryGetValue(name, out var set))
                            lineNames[name] = set = new SortedSet<int>();
                        set.Add(lineIndex);
                        i++;
                    }
                    if (i >= toks.Length) return null; // unclosed [
                    i++; // consume ]
                    continue;
                }

                if (toks[i] is FunctionToken fn && fn.Data.Isi(FunctionNames.Repeat))
                {
                    if (!TryParseRepeat(fn, out var repeatKind, out var repeatTracks)) return null;

                    if (repeatKind == GridAutoRepeatKind.None)
                    {
                        fixedTracks.AddRange(repeatTracks);
                    }
                    else
                    {
                        if (autoRepeat != GridAutoRepeatKind.None) return null;
                        autoRepeat = repeatKind;
                        autoRepeatTracks = repeatTracks;
                        autoRepeatIndex = fixedTracks.Count;
                    }

                    i++;
                    continue;
                }

                if (!TryParseTrackSize(toks[i], out var track)) return null;
                fixedTracks.Add(track);
                i++;
            }

            if (fixedTracks.Count == 0 && autoRepeat == GridAutoRepeatKind.None) return null;

            return new GridTemplate
            {
                Tracks = fixedTracks,
                AutoRepeat = autoRepeat,
                AutoRepeatTracks = autoRepeatTracks,
                AutoRepeatInsertIndex = autoRepeatIndex < 0 ? 0 : autoRepeatIndex,
                LineNames = lineNames.ToDictionary(
                    kv => kv.Key,
                    kv => (IReadOnlyList<int>)kv.Value.ToList())
            };
        }

        /// <summary><c>subgrid &lt;line-name-list&gt;?</c>, the caller matched the keyword.</summary>
        private static GridTemplate TryParseSubgrid(Token[] toks)
        {
            var lineNames = new Dictionary<string, SortedSet<int>>();
            var lineIndex = 1;
            var i = 1; // skip the subgrid ident

            while (i < toks.Length)
            {
                if (toks[i].Type != TokenType.SquareBracketOpen) return null;
                i++;
                while (i < toks.Length && toks[i].Type != TokenType.SquareBracketClose)
                {
                    if (toks[i].Type != TokenType.Ident) return null;
                    var name = toks[i].Data;
                    if (!lineNames.TryGetValue(name, out var set))
                        lineNames[name] = set = new SortedSet<int>();
                    set.Add(lineIndex);
                    i++;
                }
                if (i >= toks.Length) return null; // unclosed [
                i++; // consume ]
                lineIndex++;
            }

            return new GridTemplate
            {
                IsSubgrid = true,
                LineNames = lineNames.ToDictionary(
                    kv => kv.Key,
                    kv => (IReadOnlyList<int>)kv.Value.ToList())
            };
        }

        /// <summary><c>&lt;track-size&gt;+</c>, no <c>repeat()</c>.</summary>
        internal static IReadOnlyList<GridTrackSize> TryParseTrackSizeList(IReadOnlyList<Token> tokens)
        {
            var toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (toks.Length == 0) return null;

            var result = new List<GridTrackSize>();
            foreach (var token in toks)
            {
                if (!TryParseTrackSize(token, out var track)) return null;
                result.Add(track);
            }

            return result;
        }

        private static bool TryParseRepeat(FunctionToken fn, out GridAutoRepeatKind kind, out List<GridTrackSize> tracks)
        {
            kind = GridAutoRepeatKind.None;
            tracks = null;

            var args = fn.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            var groups = SplitByComma(args);
            if (groups.Count < 2) return false;

            var first = groups[0];
            if (first.Count != 1) return false;

            var countTokens = 0;
            if (IsIdent(first[0], Keywords.AutoFill)) kind = GridAutoRepeatKind.AutoFill;
            else if (IsIdent(first[0], Keywords.AutoFit)) kind = GridAutoRepeatKind.AutoFit;
            else if (first[0] is NumberToken n && n.IsInteger && n.IntegerValue >= 1) countTokens = n.IntegerValue;
            else return false;

            // A nested repeat() is a single function token, which TryParseTrackSize rejects.
            var body = new List<GridTrackSize>();
            for (var gi = 1; gi < groups.Count; gi++)
            {
                foreach (var token in groups[gi])
                {
                    if (!TryParseTrackSize(token, out var track)) return false;
                    body.Add(track);
                }
            }

            if (body.Count == 0) return false;

            if (kind != GridAutoRepeatKind.None)
            {
                tracks = body;
                return true;
            }

            tracks = new List<GridTrackSize>(body.Count * countTokens);
            for (var r = 0; r < countTokens; r++)
                tracks.AddRange(body);
            return true;
        }

        private static bool TryParseTrackSize(Token token, out GridTrackSize track)
        {
            track = null;

            if (token is FunctionToken fn)
            {
                if (fn.Data.Isi(FunctionNames.Minmax)) return TryParseMinmax(fn, out track);
                if (fn.Data.Isi(FunctionNames.FitContent)) return TryParseFitContent(fn, out track);
                if (IsLengthPercentageCalc(fn)) { track = GridTrackSize.Length(fn.ToValue()); return true; }
                return false;
            }

            if (TryParseBreadth(token, out track)) return true;
            return false;
        }

        private static bool TryParseBreadth(Token token, out GridTrackSize track)
        {
            track = null;

            if (token is FunctionToken calc && IsLengthPercentageCalc(calc))
            {
                track = GridTrackSize.Length(calc.ToValue());
                return true;
            }

            if (token.Type == TokenType.Ident)
            {
                if (token.Data.Isi(Keywords.Auto)) { track = GridTrackSize.Auto; return true; }
                if (token.Data.Isi(Keywords.MinContent)) { track = GridTrackSize.MinContent; return true; }
                if (token.Data.Isi(Keywords.MaxContent)) { track = GridTrackSize.MaxContent; return true; }
                return false;
            }

            if (token.Type == TokenType.Percentage) { track = GridTrackSize.Percent(token.ToValue()); return true; }

            if (token is UnitToken unit && token.Type == TokenType.Dimension)
            {
                if (unit.Unit.Isi("fr"))
                {
                    if (unit.Value < 0) return false;
                    track = GridTrackSize.FlexFactor(unit.Value);
                    return true;
                }

                track = GridTrackSize.Length(token.ToValue());
                return true;
            }

            var zero = token as NumberToken;
            if (zero != null && zero.Value == 0f) { track = GridTrackSize.Length("0"); return true; }

            return false;
        }

        private static bool TryParseMinmax(FunctionToken fn, out GridTrackSize track)
        {
            track = null;
            var groups = SplitByComma(fn.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace).ToArray());
            if (groups.Count != 2 || groups[0].Count != 1 || groups[1].Count != 1) return false;

            if (!TryParseBreadth(groups[0][0], out var min)) return false;
            if (!TryParseBreadth(groups[1][0], out var max)) return false;

            if (min.Kind == GridTrackKind.Flex) return false;

            track = GridTrackSize.Minmax(min, max);
            return true;
        }

        private static bool TryParseFitContent(FunctionToken fn, out GridTrackSize track)
        {
            track = null;
            var args = fn.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (args.Length != 1) return false;

            var arg = args[0];
            if (arg.Type == TokenType.Percentage) { track = GridTrackSize.FitContentTo(arg.ToValue()); return true; }
            if (arg is UnitToken && arg.Type == TokenType.Dimension && !((UnitToken)arg).Unit.Isi("fr"))
            {
                track = GridTrackSize.FitContentTo(arg.ToValue());
                return true;
            }
            var zeroArg = arg as NumberToken;
            if (zeroArg != null && zeroArg.Value == 0f) { track = GridTrackSize.FitContentTo("0"); return true; }
            var calc = arg as FunctionToken;
            if (calc != null && IsLengthPercentageCalc(calc))
            {
                track = GridTrackSize.FitContentTo(calc.ToValue());
                return true;
            }

            return false;
        }

        /// <summary>Whether <paramref name="fn"/> is a math function of type <c>&lt;length-percentage&gt;</c>.</summary>
        private static bool IsLengthPercentageCalc(FunctionToken fn)
        {
            if (!CalcParser.IsCalcFamily(fn.Data)) return false;

            var node = CalcParser.Parse(fn);
            if (node == null) return false;

            var category = CalcTypeChecker.Check(node);
            return category == CalcCategory.Length
                || category == CalcCategory.Percentage
                || category == CalcCategory.LengthPercentage;
        }

        private static bool IsIdent(Token token, string keyword) =>
            token.Type == TokenType.Ident && token.Data.Isi(keyword);

        private static List<List<Token>> SplitByComma(IReadOnlyList<Token> tokens)
        {
            var groups = new List<List<Token>>();
            var current = new List<Token>();
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Comma)
                {
                    groups.Add(current);
                    current = new List<Token>();
                }
                else
                {
                    current.Add(token);
                }
            }
            groups.Add(current);
            return groups;
        }
    }
}
