using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates one list entry of a scroll-driven animation / transition-behavior property against a
    /// <see cref="TimelineGrammar"/> predicate. The authored (whitespace-normalised) text is preserved.
    /// </summary>
    internal sealed class TimelineValueConverter : IValueConverter
    {
        private readonly Func<Token[], bool> _grammar;

        public TimelineValueConverter(Func<Token[], bool> grammar)
        {
            _grammar = grammar;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = TimelineGrammar.Compact(value);

            return tokens.Length != 0 && _grammar(tokens) ? new TimelineValue(tokens) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<TimelineValue>();
        }

        internal static List<Token> Join(IEnumerable<Token> tokens)
        {
            var result = new List<Token>();

            foreach (var token in tokens)
            {
                if (result.Count > 0) result.Add(Token.Whitespace);

                result.Add(token);
            }

            return result;
        }

        private sealed class TimelineValue : IPropertyValue
        {
            public TimelineValue(Token[] tokens)
            {
                Original = new TokenValue(Join(tokens));
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }

    /// <summary>
    /// One entry of the <c>animation-range</c> shorthand, <c>&lt;'animation-range-start'&gt; &lt;'animation-range-end'&gt;?</c>
    /// (Scroll-driven Animations 1 4.3). When the end is omitted and the start names a range, the end is that same range
    /// (so <c>cover</c> covers the whole of <c>cover</c>); otherwise it is <c>normal</c>.
    /// </summary>
    internal sealed class AnimationRangeValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = TimelineGrammar.Compact(value);

            return tokens.Length != 0 && TimelineGrammar.TryParseAnimationRange(tokens, out var start, out var end)
                ? new RangeValue(start, end)
                : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var start = ReadEdge(properties, PropertyNames.AnimationRangeStart);
            var end = ReadEdge(properties, PropertyNames.AnimationRangeEnd);

            if (start == null || end == null) return null;

            var omitEnd = TimelineGrammar.IsKeyword(end[0], Keywords.Normal)
                ? !TimelineGrammar.IsRangeName(start[0])
                : TimelineGrammar.IsRangeName(start[0]) && SameAsDefaultEnd(start[0], end);

            return new RangeValue(start, omitEnd ? new Token[0] : end);
        }

        private static bool SameAsDefaultEnd(Token startName, Token[] end)
        {
            return end[0].Data.Isi(startName.Data) &&
                   (end.Length == 1 || end.Length == 2 && end[1] is UnitToken {Type: TokenType.Percentage} percent &&
                       Math.Abs(percent.Value - 100f) < 0.0001f);
        }

        private static Token[] ReadEdge(IEnumerable<Property> properties, string name)
        {
            var property = properties.FirstOrDefault(p => p.Name == name);
            var original = property?.DeclaredValue?.Original;

            if (original == null) return new[] {NormalToken()};

            var tokens = TimelineGrammar.Compact(original);

            return tokens.Length != 0 && TimelineGrammar.IsRangeEdge(tokens) ? tokens : null;
        }

        private static Token NormalToken() => new Token(TokenType.Ident, Keywords.Normal, TextPosition.Empty);

        private sealed class RangeValue : IPropertyValue
        {
            private readonly Token[] _start;
            private readonly Token[] _end;

            public RangeValue(Token[] start, Token[] end)
            {
                _start = start;
                _end = end;
                Original = new TokenValue(TimelineValueConverter.Join(start.Concat(end)));
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                if (name == PropertyNames.AnimationRangeStart) return new TokenValue(TimelineValueConverter.Join(_start));

                if (name != PropertyNames.AnimationRangeEnd) return null;

                if (_end.Length != 0) return new TokenValue(TimelineValueConverter.Join(_end));

                // An omitted end repeats the start's range name, else it is "normal".
                var first = _start[0];
                var implied = TimelineGrammar.IsRangeName(first)
                    ? first
                    : new Token(TokenType.Ident, Keywords.Normal, TextPosition.Empty);

                return new TokenValue(new[] {implied});
            }
        }
    }

    /// <summary>
    /// Wraps a converter so it refuses a lone keyword that another component of a shorthand owns
    /// (CSS Animations 1 3.9: keywords valid for another longhand are accepted for that longhand, not for animation-name).
    /// </summary>
    internal sealed class ExcludingKeywordValueConverter : IValueConverter
    {
        private readonly IValueConverter _converter;
        private readonly string _keyword;

        public ExcludingKeywordValueConverter(IValueConverter converter, string keyword)
        {
            _converter = converter;
            _keyword = keyword;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = TimelineGrammar.Compact(value);

            return tokens.Length == 1 && TimelineGrammar.IsKeyword(tokens[0], _keyword) ? null : _converter.Convert(value);
        }

        public IPropertyValue Construct(Property[] properties) => _converter.Construct(properties);
    }

    /// <summary>The ready-made converters of the scroll-driven animation properties.</summary>
    internal static class TimelineConverters
    {
        private static IValueConverter Entry(Func<Token[], bool> grammar) => new TimelineValueConverter(grammar);

        internal static readonly IValueConverter TransitionBehavior = Entry(TimelineGrammar.IsTransitionBehavior);
        internal static readonly IValueConverter AnimationComposition = Entry(TimelineGrammar.IsAnimationComposition);
        internal static readonly IValueConverter AnimationTimeline = Entry(TimelineGrammar.IsAnimationTimeline);
        internal static readonly IValueConverter ShorthandTimeline = Entry(TimelineGrammar.IsShorthandTimeline);
        // animation-name in the animation shorthand: any identifier except auto, which is the timeline's.
        internal static readonly IValueConverter ShorthandAnimationName =
            new ExcludingKeywordValueConverter(Converters.AnimationNameConverter, Keywords.Auto);

        internal static readonly IValueConverter RangeEdge =Entry(TimelineGrammar.IsRangeEdge);
        internal static readonly IValueConverter TimelineName = Entry(TimelineGrammar.IsTimelineName);
        internal static readonly IValueConverter TimelineAxis = Entry(TimelineGrammar.IsAxisEntry);
        internal static readonly IValueConverter TimelineInset = Entry(TimelineGrammar.IsInset);

        // none | all | <dashed-ident>#
        internal static readonly IValueConverter TimelineScope =
            Entry(TimelineGrammar.IsDashedIdentEntry).FromList().Or(Keywords.None).Or(Keywords.All).OrDefault();
    }
}
