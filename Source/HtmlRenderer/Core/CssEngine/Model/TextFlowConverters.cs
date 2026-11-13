using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Keyword maps and value converters for the writing-mode and text-wrapping properties
    /// (CSS Writing Modes 4, CSS Text 3/4): <c>writing-mode</c>, <c>text-orientation</c>,
    /// <c>text-combine-upright</c>, <c>line-break</c>, <c>white-space-collapse</c>, <c>text-wrap-mode</c>,
    /// <c>text-wrap-style</c>, <c>text-align-all</c>, and the <c>white-space</c>, <c>text-wrap</c> and
    /// <c>text-align</c> shorthands.
    /// </summary>
    internal static class TextFlowConverters
    {
        private const string Collapse = "collapse";
        private const string Preserve = "preserve";
        private const string PreserveBreaks = "preserve-breaks";
        private const string PreserveSpaces = "preserve-spaces";
        private const string BreakSpaces = "break-spaces";
        private const string Wrap = "wrap";
        private const string Balance = "balance";
        private const string Stable = "stable";
        private const string Pretty = "pretty";
        private const string AvoidOrphans = "avoid-orphans";
        private const string MatchParent = "match-parent";
        private const string JustifyAll = "justify-all";
        private const string Digits = "digits";

        public static readonly Dictionary<string, WritingMode> WritingModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {"horizontal-tb", WritingMode.HorizontalTb},
                {"vertical-rl", WritingMode.VerticalRl},
                {"vertical-lr", WritingMode.VerticalLr},
                {"sideways-rl", WritingMode.SidewaysRl},
                {"sideways-lr", WritingMode.SidewaysLr}
            };

        public static readonly Dictionary<string, TextOrientation> TextOrientations =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {"mixed", TextOrientation.Mixed},
                {"upright", TextOrientation.Upright},
                {"sideways", TextOrientation.Sideways}
            };

        public static readonly Dictionary<string, LineBreak> LineBreaks =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Auto, LineBreak.Auto},
                {"loose", LineBreak.Loose},
                {Keywords.Normal, LineBreak.Normal},
                {"strict", LineBreak.Strict},
                {"anywhere", LineBreak.Anywhere}
            };

        public static readonly Dictionary<string, WhiteSpaceCollapse> WhiteSpaceCollapses =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Collapse, WhiteSpaceCollapse.Collapse},
                {Preserve, WhiteSpaceCollapse.Preserve},
                {PreserveBreaks, WhiteSpaceCollapse.PreserveBreaks},
                {PreserveSpaces, WhiteSpaceCollapse.PreserveSpaces},
                {BreakSpaces, WhiteSpaceCollapse.BreakSpaces}
            };

        public static readonly Dictionary<string, TextWrapMode> TextWrapModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Wrap, TextWrapMode.Wrap},
                {Keywords.Nowrap, TextWrapMode.NoWrap}
            };

        public static readonly Dictionary<string, TextWrapStyle> TextWrapStyles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Auto, TextWrapStyle.Auto},
                {Balance, TextWrapStyle.Balance},
                {Stable, TextWrapStyle.Stable},
                {Pretty, TextWrapStyle.Pretty},
                {AvoidOrphans, TextWrapStyle.AvoidOrphans}
            };

        public static readonly Dictionary<string, TextAlignAll> TextAlignAlls =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Start, TextAlignAll.Start},
                {Keywords.End, TextAlignAll.End},
                {Keywords.Left, TextAlignAll.Left},
                {Keywords.Right, TextAlignAll.Right},
                {Keywords.Center, TextAlignAll.Center},
                {Keywords.Justify, TextAlignAll.Justify},
                {MatchParent, TextAlignAll.MatchParent}
            };

        public static readonly IValueConverter WritingModeConverter = WritingModes.ToConverter().OrDefault();
        public static readonly IValueConverter TextOrientationConverter = TextOrientations.ToConverter().OrDefault();
        public static readonly IValueConverter LineBreakConverter = LineBreaks.ToConverter().OrDefault();
        public static readonly IValueConverter WhiteSpaceCollapseConverter = WhiteSpaceCollapses.ToConverter().OrDefault();
        public static readonly IValueConverter TextWrapModeConverter = TextWrapModes.ToConverter().OrDefault();
        public static readonly IValueConverter TextWrapStyleConverter = TextWrapStyles.ToConverter().OrDefault();
        public static readonly IValueConverter TextAlignAllConverter = TextAlignAlls.ToConverter().OrDefault();
        public static readonly IValueConverter TextCombineUprightConverter = new TextCombineUprightValueConverter();

        // white-space (CSS Text 4 4.1): the legacy keywords are fixed <white-space-collapse> / <text-wrap-mode>
        // pairs; otherwise the grammar is "<'white-space-collapse'> || <'text-wrap-mode'>".
        public static readonly IValueConverter WhiteSpaceConverter = new WhiteSpaceShorthandConverter();

        // text-wrap (CSS Text 4 5.1): "<'text-wrap-mode'> || <'text-wrap-style'>".
        public static readonly IValueConverter TextWrapConverter = new TextWrapShorthandConverter();

        // text-align (CSS Text 3 6.1): <'text-align-all'> plus justify-all, which also sets text-align-last.
        public static readonly IValueConverter TextAlignConverter = new TextAlignShorthandConverter();

        private static readonly string[][] WhiteSpacePairs =
        {
            new[] {Keywords.Normal, Collapse, Wrap},
            new[] {Keywords.Pre, Preserve, Keywords.Nowrap},
            new[] {Keywords.PreWrap, Preserve, Wrap},
            new[] {Keywords.PreLine, PreserveBreaks, Wrap},
            new[] {Keywords.Nowrap, Collapse, Keywords.Nowrap}
        };

        private static bool IsGlobal(string keyword)
        {
            return keyword == Keywords.Inherit || keyword == Keywords.Initial || keyword == Keywords.Unset ||
                   keyword == Keywords.Revert || keyword == Keywords.RevertLayer;
        }

        private static List<string> Idents(IEnumerable<Token> value)
        {
            var list = new List<string>();

            foreach (var token in value)
            {
                if (token.Type == TokenType.Whitespace) continue;
                if (token.Type != TokenType.Ident) return null;
                list.Add(token.Data.ToLowerInvariant());
            }

            return list.Count == 0 ? null : list;
        }

        private static IReadOnlyList<Token> Tokens(string text)
        {
            var tokens = new List<Token>();

            foreach (var part in text.Split(' '))
            {
                if (tokens.Count > 0) tokens.Add(Token.Whitespace);
                tokens.Add(new Token(TokenType.Ident, part, TextPosition.Empty));
            }

            return tokens;
        }

        private static IPropertyValue Pair(string cssText, string firstName, string firstValue,
            string secondName, string secondValue)
        {
            var map = new Dictionary<string, IReadOnlyList<Token>>
            {
                {firstName, Tokens(firstValue)},
                {secondName, Tokens(secondValue)}
            };

            return new MappedShorthandValue(map, Tokens(cssText));
        }

        private static Property Find(Property[] properties, string name)
        {
            return properties.FirstOrDefault(p => p.Name == name);
        }

        private sealed class WhiteSpaceShorthandConverter : IValueConverter
        {
            public IPropertyValue Convert(IEnumerable<Token> value)
            {
                var idents = Idents(value);
                if (idents == null || idents.Count > 2) return null;

                if (idents.Count == 1)
                {
                    var keyword = idents[0];

                    if (IsGlobal(keyword))
                        return Pair(keyword, PropertyNames.WhiteSpaceCollapse, keyword, PropertyNames.TextWrapMode, keyword);

                    foreach (var pair in WhiteSpacePairs)
                    {
                        if (pair[0] == keyword)
                            return Pair(keyword, PropertyNames.WhiteSpaceCollapse, pair[1], PropertyNames.TextWrapMode, pair[2]);
                    }
                }

                string collapse = null;
                string mode = null;

                foreach (var keyword in idents)
                {
                    if (WhiteSpaceCollapses.ContainsKey(keyword) && collapse == null) collapse = keyword;
                    else if (TextWrapModes.ContainsKey(keyword) && mode == null) mode = keyword;
                    else return null;
                }

                collapse ??= Collapse;
                mode ??= Wrap;

                return Pair(string.Join(" ", idents), PropertyNames.WhiteSpaceCollapse, collapse,
                    PropertyNames.TextWrapMode, mode);
            }

            public IPropertyValue Construct(Property[] properties)
            {
                var collapse = Find(properties, PropertyNames.WhiteSpaceCollapse);
                var mode = Find(properties, PropertyNames.TextWrapMode);
                if (collapse == null || mode == null || !collapse.HasValue || !mode.HasValue) return null;

                var collapseValue = collapse.Value;
                var modeValue = mode.Value;

                if (IsGlobal(collapseValue) || IsGlobal(modeValue))
                {
                    return collapseValue == modeValue
                        ? Pair(collapseValue, PropertyNames.WhiteSpaceCollapse, collapseValue,
                            PropertyNames.TextWrapMode, modeValue)
                        : null;
                }

                if (!WhiteSpaceCollapses.ContainsKey(collapseValue) || !TextWrapModes.ContainsKey(modeValue))
                    return null;

                foreach (var pair in WhiteSpacePairs)
                {
                    if (pair[1] == collapseValue && pair[2] == modeValue)
                        return Pair(pair[0], PropertyNames.WhiteSpaceCollapse, collapseValue,
                            PropertyNames.TextWrapMode, modeValue);
                }

                var text = modeValue == Wrap ? collapseValue : collapseValue + " " + modeValue;
                return Pair(text, PropertyNames.WhiteSpaceCollapse, collapseValue, PropertyNames.TextWrapMode, modeValue);
            }
        }

        private sealed class TextWrapShorthandConverter : IValueConverter
        {
            public IPropertyValue Convert(IEnumerable<Token> value)
            {
                var idents = Idents(value);
                if (idents == null || idents.Count > 2) return null;

                if (idents.Count == 1 && IsGlobal(idents[0]))
                {
                    return Pair(idents[0], PropertyNames.TextWrapMode, idents[0], PropertyNames.TextWrapStyle, idents[0]);
                }

                string mode = null;
                string style = null;

                foreach (var keyword in idents)
                {
                    if (TextWrapModes.ContainsKey(keyword) && mode == null) mode = keyword;
                    else if (TextWrapStyles.ContainsKey(keyword) && style == null) style = keyword;
                    else return null;
                }

                return Pair(string.Join(" ", idents), PropertyNames.TextWrapMode, mode ?? Wrap,
                    PropertyNames.TextWrapStyle, style ?? Keywords.Auto);
            }

            public IPropertyValue Construct(Property[] properties)
            {
                var mode = Find(properties, PropertyNames.TextWrapMode);
                var style = Find(properties, PropertyNames.TextWrapStyle);
                if (mode == null || style == null || !mode.HasValue || !style.HasValue) return null;

                var modeValue = mode.Value;
                var styleValue = style.Value;

                if (IsGlobal(modeValue) || IsGlobal(styleValue))
                {
                    return modeValue == styleValue
                        ? Pair(modeValue, PropertyNames.TextWrapMode, modeValue, PropertyNames.TextWrapStyle, styleValue)
                        : null;
                }

                if (!TextWrapModes.ContainsKey(modeValue) || !TextWrapStyles.ContainsKey(styleValue)) return null;

                // Serialize the shortest form: a longhand at its initial value is omitted.
                var parts = new List<string>();
                if (modeValue != Wrap) parts.Add(modeValue);
                if (styleValue != Keywords.Auto) parts.Add(styleValue);
                var text = parts.Count == 0 ? Wrap : string.Join(" ", parts);

                return Pair(text, PropertyNames.TextWrapMode, modeValue, PropertyNames.TextWrapStyle, styleValue);
            }
        }

        private sealed class TextAlignShorthandConverter : IValueConverter
        {
            public IPropertyValue Convert(IEnumerable<Token> value)
            {
                var idents = Idents(value);
                if (idents == null || idents.Count != 1) return null;

                var keyword = idents[0];

                if (IsGlobal(keyword))
                    return Pair(keyword, PropertyNames.TextAlignAll, keyword, PropertyNames.TextAlignLast, keyword);

                // justify-all sets both longhands; every other keyword sets text-align-all and resets
                // text-align-last to its initial value.
                if (keyword == JustifyAll)
                    return Pair(keyword, PropertyNames.TextAlignAll, Keywords.Justify,
                        PropertyNames.TextAlignLast, Keywords.Justify);

                return TextAlignAlls.ContainsKey(keyword)
                    ? Pair(keyword, PropertyNames.TextAlignAll, keyword, PropertyNames.TextAlignLast, Keywords.Auto)
                    : null;
            }

            public IPropertyValue Construct(Property[] properties)
            {
                var all = Find(properties, PropertyNames.TextAlignAll);
                var last = Find(properties, PropertyNames.TextAlignLast);
                if (all == null || last == null || !all.HasValue || !last.HasValue) return null;

                var allValue = all.Value;
                var lastValue = last.Value;

                if (IsGlobal(allValue) || IsGlobal(lastValue))
                {
                    return allValue == lastValue
                        ? Pair(allValue, PropertyNames.TextAlignAll, allValue, PropertyNames.TextAlignLast, lastValue)
                        : null;
                }

                if (!TextAlignAlls.ContainsKey(allValue)) return null;

                if (allValue == Keywords.Justify && lastValue == Keywords.Justify)
                    return Pair(JustifyAll, PropertyNames.TextAlignAll, allValue, PropertyNames.TextAlignLast, lastValue);

                return lastValue == Keywords.Auto
                    ? Pair(allValue, PropertyNames.TextAlignAll, allValue, PropertyNames.TextAlignLast, lastValue)
                    : null;
            }
        }

        // none | all | digits <integer [2,4]>? (CSS Writing Modes 4 9.1)
        private sealed class TextCombineUprightValueConverter : IValueConverter
        {
            public IPropertyValue Convert(IEnumerable<Token> value)
            {
                var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

                if (tokens.Length == 1 && tokens[0].Type == TokenType.Ident)
                {
                    var keyword = tokens[0].Data.ToLowerInvariant();

                    return keyword == Keywords.None || keyword == Keywords.All || keyword == Digits || IsGlobal(keyword)
                        ? new CombineValue(keyword)
                        : null;
                }

                if (tokens.Length == 2 && tokens[0].Type == TokenType.Ident &&
                    tokens[0].Data.ToLowerInvariant() == Digits &&
                    tokens[1] is NumberToken {IsInteger: true, IntegerValue: >= 2 and <= 4} number)
                {
                    return new CombineValue(Digits + " " + number.IntegerValue.ToString(
                        System.Globalization.CultureInfo.InvariantCulture));
                }

                return null;
            }

            public IPropertyValue Construct(Property[] properties)
            {
                return properties.Guard<CombineValue>();
            }

            private sealed class CombineValue : IPropertyValue
            {
                public CombineValue(string text)
                {
                    CssText = text;
                    Original = new TokenValue(Tokens(text));
                }

                public string CssText { get; }

                public TokenValue Original { get; }

                public TokenValue ExtractFor(string name) => Original;
            }
        }
    }
}
