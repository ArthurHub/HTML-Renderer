using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// <c>text-emphasis</c> (CSS Text Decoration 4 4.4): the shorthand
    /// <c>&lt;'text-emphasis-style'&gt; || &lt;'text-emphasis-color'&gt;</c>. The style may itself be several
    /// tokens (<c>filled circle</c>) and the colour a function, so the value is split at the one boundary
    /// between them; an omitted longhand resets to its initial value.
    /// </summary>
    internal sealed class TextEmphasisProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = new TextEmphasisShorthandConverter();

        internal TextEmphasisProperty()
            : base(PropertyNames.TextEmphasis, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;

        private sealed class TextEmphasisShorthandConverter : IValueConverter
        {
            public IPropertyValue Convert(IEnumerable<Token> value)
            {
                // Keep the whitespace between a component's own tokens ("filled circle" must stay two words).
                var raw = new List<Token>(value);
                var significant = new List<int>();

                for (var i = 0; i < raw.Count; i++)
                {
                    if (raw[i].Type != TokenType.Whitespace) significant.Add(i);
                }

                if (significant.Count == 0) return null;

                List<Token> Slice(int first, int to) =>
                    first >= to
                        ? new List<Token>()
                        : raw.GetRange(significant[first], significant[to - 1] - significant[first] + 1);

                // The style and the colour each occupy one contiguous run, in either order, so the only
                // split that can work is a prefix/suffix one.
                for (var split = 0; split <= significant.Count; split++)
                {
                    var head = Slice(0, split);
                    var tail = Slice(split, significant.Count);

                    if (Accepts(head, tail, out var style, out var color))
                        return new EmphasisValue(style, color, Slice(0, significant.Count));
                }

                return null;
            }

            private static bool IsStyle(List<Token> tokens) => TextEmphasisGrammar.TryParseStyle(tokens);

            private static bool IsColor(List<Token> tokens) => Converters.ColorConverter.Convert(tokens) != null;

            private static bool Accepts(List<Token> head, List<Token> tail, out List<Token> style,
                out List<Token> color)
            {
                style = null;
                color = null;

                if (head.Count == 0)
                {
                    // Only one component, either of the two.
                    if (IsStyle(tail))
                    {
                        style = tail;
                        return true;
                    }

                    if (IsColor(tail))
                    {
                        color = tail;
                        return true;
                    }

                    return false;
                }

                if (tail.Count == 0) return false;

                if (IsStyle(head) && IsColor(tail))
                {
                    style = head;
                    color = tail;
                    return true;
                }

                if (IsColor(head) && IsStyle(tail))
                {
                    style = tail;
                    color = head;
                    return true;
                }

                return false;
            }

            public IPropertyValue Construct(Property[] properties)
            {
                var style = properties.FirstOrDefault(p => p.Name.Is(PropertyNames.TextEmphasisStyle));
                var color = properties.FirstOrDefault(p => p.Name.Is(PropertyNames.TextEmphasisColor));
                var parts = new List<string>();

                if (style is { HasValue: true } && !style.IsInitial) parts.Add(style.Value);
                if (color is { HasValue: true } && !color.IsInitial) parts.Add(color.Value);

                return parts.Count == 0 ? null : new TextValue(string.Join(" ", parts));
            }

            private sealed class TextValue : IPropertyValue
            {
                public TextValue(string text)
                {
                    CssText = text;
                }

                public string CssText { get; }

                public TokenValue Original => TokenValue.FromString(CssText);

                public TokenValue ExtractFor(string name) => Original;
            }

            private sealed class EmphasisValue : IPropertyValue
            {
                private readonly TokenValue _style;
                private readonly TokenValue _color;

                public EmphasisValue(List<Token> style, List<Token> color, List<Token> all)
                {
                    _style = style == null ? null : new TokenValue(style);
                    _color = color == null ? null : new TokenValue(color);
                    Original = new TokenValue(all);
                }

                public string CssText => Original.Text;

                public TokenValue Original { get; }

                public TokenValue ExtractFor(string name) =>
                    name.Is(PropertyNames.TextEmphasisStyle) ? _style
                    : name.Is(PropertyNames.TextEmphasisColor) ? _color
                    : null;
            }
        }
    }
}
