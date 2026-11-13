using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The kind of a single grid <c>&lt;track-size&gt;</c>/<c>&lt;track-breadth&gt;</c>
    /// (CSS Grid Layout Module Level 1/2 §7.2).</summary>
    internal enum GridTrackKind
    {
        Length,       // a <length> (raw component string in Value)
        Percent,      // a <percentage> (raw component string in Value)
        Flex,         // a <flex> / fr value (Flex holds the fr number)
        Auto,
        MinContent,
        MaxContent,
        FitContent,   // fit-content(<length-percentage>) — the argument is in Value
        Minmax        // minmax(Min, Max)
    }

    /// <summary>Which flavour of layout-time <c>repeat()</c> a template carries (CSS Grid §7.2.3.2).</summary>
    internal enum GridAutoRepeatKind { None, AutoFill, AutoFit }

    /// <summary>
    /// A single grid <c>&lt;track-size&gt;</c>. Fixed/percentage/fit-content values keep their authored
    /// component string (<see cref="Value"/>); <c>fr</c> keeps its numeric factor (<see cref="Flex"/>);
    /// <c>minmax()</c> keeps its two sub-breadths.
    /// </summary>
    internal sealed class GridTrackSize
    {
        public GridTrackKind Kind { get; private set; }
        public string Value { get; private set; }
        public double Flex { get; private set; }
        public GridTrackSize Min { get; private set; }
        public GridTrackSize Max { get; private set; }

        public static GridTrackSize Length(string v) => new() { Kind = GridTrackKind.Length, Value = v };
        public static GridTrackSize Percent(string v) => new() { Kind = GridTrackKind.Percent, Value = v };
        public static GridTrackSize FlexFactor(double fr) => new() { Kind = GridTrackKind.Flex, Flex = fr };
        public static readonly GridTrackSize Auto = new() { Kind = GridTrackKind.Auto };
        public static readonly GridTrackSize MinContent = new() { Kind = GridTrackKind.MinContent };
        public static readonly GridTrackSize MaxContent = new() { Kind = GridTrackKind.MaxContent };
        public static GridTrackSize FitContentTo(string v) => new() { Kind = GridTrackKind.FitContent, Value = v };
        public static GridTrackSize Minmax(GridTrackSize min, GridTrackSize max) =>
            new() { Kind = GridTrackKind.Minmax, Min = min, Max = max };
    }

    /// <summary>
    /// A parsed <c>grid-template-columns</c>/<c>grid-template-rows</c> value: the fixed (fully
    /// <c>repeat(N,…)</c>-expanded) tracks, plus at most one layout-time <c>repeat(auto-fill|auto-fit,…)</c>
    /// section recorded as an insertion point since its count is resolved during layout.
    /// </summary>
    internal sealed class GridTemplate
    {
        /// <summary>The fixed tracks (with the auto-repeat section, if any, NOT included here — it is
        /// spliced in at <see cref="AutoRepeatInsertIndex"/> at layout time).</summary>
        public IReadOnlyList<GridTrackSize> Tracks { get; internal set; } = new List<GridTrackSize>();

        public GridAutoRepeatKind AutoRepeat { get; internal set; } = GridAutoRepeatKind.None;

        /// <summary>The repeated track template for an <c>auto-fill</c>/<c>auto-fit</c> section.</summary>
        public IReadOnlyList<GridTrackSize> AutoRepeatTracks { get; internal set; }

        /// <summary>The index into <see cref="Tracks"/> at which the auto-repeat section is inserted.</summary>
        public int AutoRepeatInsertIndex { get; internal set; }

        /// <summary>Named lines declared with <c>[name]</c> in the top-level track list: name → the sorted,
        /// deduped 1-based line numbers it labels (line 1 is before the first track). Empty when none.</summary>
        public IReadOnlyDictionary<string, IReadOnlyList<int>> LineNames { get; internal set; }
            = new Dictionary<string, IReadOnlyList<int>>();

        /// <summary>The <c>subgrid</c> keyword (CSS Grid Layout Module Level 2 §9): this axis adopts the parent
        /// grid's tracks instead of defining its own. A subgrid template carries no <see cref="Tracks"/> — only
        /// any explicit <c>[name]</c> line names declared after the keyword (in <see cref="LineNames"/>).</summary>
        public bool IsSubgrid { get; internal set; }

        public bool IsNone => !IsSubgrid && Tracks.Count == 0 && AutoRepeat == GridAutoRepeatKind.None;
    }

    /// <summary>
    /// Shared grammar for the grid <c>&lt;track-list&gt;</c> (<c>grid-template-columns</c>/
    /// <c>grid-template-rows</c>) and <c>&lt;track-size&gt;+</c> (<c>grid-auto-columns</c>/
    /// <c>grid-auto-rows</c>). Mirrors the ExCSS grammar-helper precedent so the grammar is defined once.
    /// Named lines (<c>[name]</c>) and <c>grid-template-areas</c> are out of the fixed track-list scope
    /// here and rejected.
    /// </summary>
    internal static class GridTrackListGrammar
    {
        /// <summary>
        /// Parses a <c>&lt;track-list&gt;</c> (the <c>grid-template-columns</c>/<c>-rows</c> value). Returns
        /// null when the value is not a valid track list — <b>including the literal <c>none</c></b>, which
        /// the property accepts separately.
        /// </summary>
        internal static GridTemplate TryParse(IReadOnlyList<Token> tokens)
        {
            var toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (toks.Length == 0) return null;

            // subgrid (CSS Grid Level 2 §9): the axis adopts the parent grid's tracks. The keyword may be
            // followed by an optional <line-name-list> ([name] groups only; repeat() of line names is a v1
            // deferral, mirroring the "named lines inside repeat()" restriction below). No <track-size> is
            // allowed after subgrid.
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
                // [name …] — one or more named lines at the current line position (1-based; line 1 is before
                // the first track). Whitespace is already stripped, so the bracket group is contiguous.
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
                        // repeat(N, …) — expand inline.
                        fixedTracks.AddRange(repeatTracks);
                    }
                    else
                    {
                        // repeat(auto-fill|auto-fit, …) — at most one per track list.
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

        /// <summary>
        /// Parses a <c>subgrid [ &lt;line-name-list&gt; ]?</c> value (the leading <c>subgrid</c> ident already
        /// matched by the caller). The optional line-name list is a run of <c>[name …]</c> bracket groups, one
        /// group per grid line (line 1 is before the first adopted track); each group's names are recorded at
        /// that 1-based line index. Anything else after <c>subgrid</c> (a track size, an unclosed/ill-formed
        /// bracket, a non-ident inside <c>[ ]</c>) is invalid.
        /// </summary>
        private static GridTemplate TryParseSubgrid(Token[] toks)
        {
            var lineNames = new Dictionary<string, SortedSet<int>>();
            var lineIndex = 1;
            var i = 1; // skip the subgrid ident

            while (i < toks.Length)
            {
                if (toks[i].Type != TokenType.SquareBracketOpen) return null; // only [name] groups may follow
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

        /// <summary>
        /// Parses a <c>&lt;track-size&gt;+</c> list (<c>grid-auto-columns</c>/<c>grid-auto-rows</c>) — one or
        /// more track sizes, no <c>repeat()</c>. Returns null on any invalid token.
        /// </summary>
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

            // First group is the repetition count: an integer, or auto-fill / auto-fit.
            var first = groups[0];
            if (first.Count != 1) return false;

            var countTokens = 0;
            if (IsIdent(first[0], Keywords.AutoFill)) kind = GridAutoRepeatKind.AutoFill;
            else if (IsIdent(first[0], Keywords.AutoFit)) kind = GridAutoRepeatKind.AutoFit;
            else if (first[0] is NumberToken { IsInteger: true } n && n.IntegerValue >= 1) countTokens = n.IntegerValue;
            else return false;

            // Remaining groups are the repeated <track-size>s. (A repeat body may not itself contain a
            // repeat(), which SplitByComma naturally enforces since a nested function is one token.)
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

            // repeat(N, body) — expand N copies now.
            tracks = new List<GridTrackSize>(body.Count * countTokens);
            for (var r = 0; r < countTokens; r++)
                tracks.AddRange(body);
            return true;
        }

        private static bool TryParseTrackSize(Token token, out GridTrackSize track)
        {
            track = null;

            // minmax(<inflexible-breadth>, <track-breadth>) and fit-content(<length-percentage>).
            if (token is FunctionToken fn)
            {
                if (fn.Data.Isi(FunctionNames.Minmax)) return TryParseMinmax(fn, out track);
                if (fn.Data.Isi(FunctionNames.FitContent)) return TryParseFitContent(fn, out track);
                return TryParseBreadth(token, out track);
            }

            if (TryParseBreadth(token, out track)) return true;
            return false;
        }

        private static bool TryParseBreadth(Token token, out GridTrackSize track)
        {
            track = null;

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

                // Any other dimension is a <length> — the raw string is preserved.
                track = GridTrackSize.Length(token.ToValue());
                return true;
            }

            // Unitless zero is a valid length.
            if (token is NumberToken { Value: 0f }) { track = GridTrackSize.Length("0"); return true; }

            if (IsMathLengthPercentage(token)) { track = GridTrackSize.Length(token.ToValue()); return true; }

            return false;
        }

        // CSS Values 4 §10, a math function resolving to a <length-percentage> is one wherever that type is
        private static bool IsMathLengthPercentage(Token token) =>
            token is FunctionToken && Converters.LengthOrPercentConverter.Convert(new[] { token }) != null;

        private static bool TryParseMinmax(FunctionToken fn, out GridTrackSize track)
        {
            track = null;
            var groups = SplitByComma(fn.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace).ToArray());
            if (groups.Count != 2 || groups[0].Count != 1 || groups[1].Count != 1) return false;

            if (!TryParseBreadth(groups[0][0], out var min)) return false;
            if (!TryParseBreadth(groups[1][0], out var max)) return false;

            // The min of a minmax() is an <inflexible-breadth> — a flex value is invalid there.
            if (min.Kind == GridTrackKind.Flex) return false;

            track = GridTrackSize.Minmax(min, max);
            return true;
        }

        private static bool TryParseFitContent(FunctionToken fn, out GridTrackSize track)
        {
            track = null;
            var args = fn.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (args.Length != 1) return false;

            // fit-content(<length-percentage>).
            var t = args[0];
            if (t.Type is TokenType.Percentage) { track = GridTrackSize.FitContentTo(t.ToValue()); return true; }
            if (t is UnitToken && t.Type == TokenType.Dimension && !((UnitToken)t).Unit.Isi("fr"))
            {
                track = GridTrackSize.FitContentTo(t.ToValue());
                return true;
            }
            if (t is NumberToken { Value: 0f }) { track = GridTrackSize.FitContentTo("0"); return true; }
            if (IsMathLengthPercentage(t)) { track = GridTrackSize.FitContentTo(t.ToValue()); return true; }

            return false;
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
