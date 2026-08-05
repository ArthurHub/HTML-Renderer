using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderRadiusConverter : IValueConverter
    {
        private readonly IValueConverter _converter = LengthOrPercentConverter.Periodic(
            PropertyNames.BorderTopLeftRadius, PropertyNames.BorderTopRightRadius,
            PropertyNames.BorderBottomRightRadius,
            PropertyNames.BorderBottomLeftRadius);

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var front = new List<Token>();
            var back = new List<Token>();
            var current = front;

            foreach (var token in value)
                if (token.Type == TokenType.Delim && token.Data.Is("/"))
                {
                    if (current == back) return null;

                    current = back;
                }
                else
                {
                    current.Add(token);
                }

            var horizontal = _converter.Convert(front);

            if (horizontal == null) return null;

            var vertical = current == back ? _converter.Convert(back) : horizontal;

            if (vertical == null) return null;

            return new BorderRadiusValue(horizontal, vertical, new TokenValue(value));
        }

        public IPropertyValue Construct(Property[] properties)
        {
            if (properties.Length == 4)
            {
                var front = new List<Token>();
                var back = new List<Token>();
                var props = new List<Property>
                {
                    properties.First(m => m.Name.Is(PropertyNames.BorderTopLeftRadius)),
                    properties.First(m => m.Name.Is(PropertyNames.BorderTopRightRadius)),
                    properties.First(
                        m => m.Name.Is(PropertyNames.BorderBottomRightRadius)),
                    properties.First(
                        m => m.Name.Is(PropertyNames.BorderBottomLeftRadius))
                };

                for (var i = 0; i < props.Count; i++)
                {
                    var dpv = props[i].DeclaredValue as IEnumerable<IPropertyValue>;

                    if (dpv == null) return null;

                    var first = dpv.First().Original;
                    var second = dpv.Last().Original;

                    if (i != 0)
                    {
                        front.Add(Token.Whitespace);
                        back.Add(Token.Whitespace);
                    }

                    front.AddRange(first);
                    back.AddRange(second);
                }

                var h = _converter.Convert(front);
                var v = _converter.Convert(back);
                var o = front.Concat(new Token(TokenType.Delim, "/", TextPosition.Empty)).Concat(back);

                return new BorderRadiusValue(h, v, new TokenValue(o));
            }

            return null;
        }

        private sealed class BorderRadiusValue : IPropertyValue
        {
            private readonly IPropertyValue _horizontal;
            private readonly IPropertyValue _vertical;

            public BorderRadiusValue(IPropertyValue horizontal, IPropertyValue vertical, TokenValue original)
            {
                _horizontal = horizontal;
                _vertical = vertical;
                Original = original;
            }

            public string CssText
            {
                get
                {
                    var horizontal = _horizontal.CssText;

                    if (_vertical == null) return horizontal;

                    var vertical = _vertical.CssText;

                    return horizontal != vertical
                        ? string.Concat(horizontal, " / ", vertical)
                        : horizontal;
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                var h = _horizontal.ExtractFor(name);
                var v = _vertical.ExtractFor(name);
                return new TokenValue(h.Concat(Token.Whitespace).Concat(v));
            }
        }
    }
}