using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OneOrMoreValueConverter : IValueConverter
    {
        private readonly IValueConverter _converter;
        private readonly int _maximum;
        private readonly int _minimum;

        public OneOrMoreValueConverter(IValueConverter converter, int minimum, int maximum)
        {
            _converter = converter;
            _minimum = minimum;
            _maximum = maximum;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var items = value.ToItems();
            var n = items.Count;

            if (n < _minimum || n > _maximum) return null;
            var values = new IPropertyValue[items.Count];

            for (var i = 0; i < n; i++)
            {
                values[i] = _converter.Convert(items[i]);

                if (values[i] == null) return null;
            }

            return new MultipleValue(values, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var result = properties.Guard<MultipleValue>();

            if (result == null)
            {
                var values = new IPropertyValue[properties.Length];

                for (var i = 0; i < properties.Length; i++)
                {
                    var value = _converter.Construct(new[] { properties[i] });

                    if (value == null) return null;

                    values[i] = value;
                }

                result = new MultipleValue(values, Enumerable.Empty<Token>());
            }

            return result;
        }

        private sealed class MultipleValue : IPropertyValue
        {
            private readonly IPropertyValue[] _values;

            public MultipleValue(IPropertyValue[] values, IEnumerable<Token> tokens)
            {
                _values = values;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get
                {
                    return string.Join(" ",
                        _values.Where(m => !string.IsNullOrEmpty(m.CssText)).Select(m => m.CssText));
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                var tokens = new List<Token>();

                foreach (var value in _values)
                {
                    var extracted = value.ExtractFor(name);

                    if (extracted != null)
                    {
                        if (tokens.Count > 0) tokens.Add(Token.Whitespace);

                        tokens.AddRange(extracted);
                    }
                }

                return new TokenValue(tokens);
            }
        }
    }
}