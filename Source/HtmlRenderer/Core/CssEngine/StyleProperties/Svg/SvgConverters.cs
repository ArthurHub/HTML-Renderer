using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Value grammars for the SVG presentation properties (SVG 2, CSS Inline Layout 3, CSS Transforms 1).
    /// </summary>
    internal static class SvgConverters
    {
        private static IValueConverter KeywordsOf(params string[] keywords)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var keyword in keywords) map[keyword] = keyword;
            return map.ToConverter();
        }

        // none | <url>
        public static readonly IValueConverter MarkerConverter = Converters.UrlConverter.OrNone();

        // auto | sRGB | linearRGB
        public static readonly IValueConverter ColorInterpolationConverter =
            KeywordsOf("auto", "sRGB", "linearRGB");

        // auto | text-bottom | alphabetic | ideographic | middle | central | mathematical | hanging | text-top
        public static readonly IValueConverter DominantBaselineConverter =
            KeywordsOf("auto", "text-bottom", "alphabetic", "ideographic", "middle", "central", "mathematical",
                "hanging", "text-top");

        // baseline | text-bottom | alphabetic | ideographic | middle | central | mathematical | hanging | text-top,
        // plus the SVG 1.1 / SVG 2 spellings auto, before-edge, text-before-edge, after-edge, text-after-edge.
        public static readonly IValueConverter AlignmentBaselineConverter =
            KeywordsOf("auto", "baseline", "text-bottom", "alphabetic", "ideographic", "middle", "central",
                "mathematical", "hanging", "text-top", "before-edge", "text-before-edge", "after-edge",
                "text-after-edge");

        // baseline | sub | super | top | center | bottom | <length-percentage>
        public static readonly IValueConverter BaselineShiftConverter =
            Converters.LengthOrPercentConverter.Or(KeywordsOf("baseline", "sub", "super", "top", "center", "bottom"));

        // content-box | border-box | fill-box | stroke-box | view-box
        public static readonly IValueConverter TransformBoxConverter =
            KeywordsOf("content-box", "border-box", "fill-box", "stroke-box", "view-box");

        public static readonly IValueConverter NonNegativeLengthOrPercentConverter =
            new NonNegativeValueConverter(Converters.LengthOrPercentConverter);

        public static readonly IValueConverter AutoNonNegativeLengthOrPercentConverter =
            NonNegativeLengthOrPercentConverter.OrAuto();

        // normal | [ fill || stroke || markers ]
        public static readonly IValueConverter PaintOrderConverter = new PaintOrderValueConverter();

        // none | path( [ <fill-rule>, ]? <string> )
        public static readonly IValueConverter PathDataConverter = new PathDataValueConverter();

        private sealed class NonNegativeValueConverter : IValueConverter
        {
            private readonly IValueConverter _converter;

            public NonNegativeValueConverter(IValueConverter converter)
            {
                _converter = converter;
            }

            public IPropertyValue Convert(IEnumerable<Token> value)
            {
                var tokens = value.ToArray();
                var first = tokens.FirstOrDefault(t => t.Type != TokenType.Whitespace);

                if (first is UnitToken { Value: < 0f } or NumberToken { Value: < 0f }) return null;

                return _converter.Convert(tokens);
            }

            public IPropertyValue Construct(Property[] properties) => _converter.Construct(properties);
        }

        private sealed class PaintOrderValueConverter : IValueConverter
        {
            public IPropertyValue Convert(IEnumerable<Token> value)
            {
                var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

                if (tokens.Length == 0 || tokens.Length > 3 || tokens.Any(t => t.Type != TokenType.Ident))
                    return null;

                if (tokens.Length == 1 && tokens[0].Data.Isi("normal")) return new SvgValue(value);

                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var token in tokens)
                {
                    if (!token.Data.Isi("fill") && !token.Data.Isi("stroke") && !token.Data.Isi("markers"))
                        return null;

                    if (!seen.Add(token.Data)) return null;
                }

                return new SvgValue(value);
            }

            public IPropertyValue Construct(Property[] properties) => properties.Guard<SvgValue>();
        }

        private sealed class PathDataValueConverter : IValueConverter
        {
            public IPropertyValue Convert(IEnumerable<Token> value)
            {
                var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

                if (tokens.Length != 1) return null;

                if (tokens[0].Type == TokenType.Ident && tokens[0].Data.Isi("none")) return new SvgValue(value);

                if (tokens[0] is not FunctionToken function || !function.Data.Isi("path")) return null;

                var args = function.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
                var index = 0;

                if (args.Length == 3 && args[0].Type == TokenType.Ident &&
                    (args[0].Data.Isi("nonzero") || args[0].Data.Isi("evenodd")) &&
                    args[1].Type == TokenType.Comma)
                {
                    index = 2;
                }

                // A bad (newline-terminated) string leaves further tokens behind, so the single-argument check
                // already rejects it. Deliberately not reading StringToken.IsValid: its polarity has differed
                // between lexer versions.
                if (args.Length - index != 1 || args[index] is not StringToken path) return null;

                return IsPathData(path.Data) ? new SvgValue(value) : null;
            }

            public IPropertyValue Construct(Property[] properties) => properties.Guard<SvgValue>();

            // Basic validation only: a non-empty path that starts with a moveto and uses only path commands,
            // numbers and separators. The full SVG path grammar is left to the consumer.
            private static bool IsPathData(string data)
            {
                var text = data.Trim();

                if (text.Length == 0 || (text[0] != 'M' && text[0] != 'm')) return false;

                foreach (var c in text)
                {
                    if (!char.IsWhiteSpace(c) && "MmLlHhVvCcSsQqTtAaZz0123456789.,+-eE".IndexOf(c) < 0)
                        return false;
                }

                return true;
            }
        }

        private sealed class SvgValue : IPropertyValue
        {
            public SvgValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
