using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class EndListValueConverter : IValueConverter
    {
        private readonly IValueConverter _endConverter;
        private readonly IValueConverter _listConverter;

        public EndListValueConverter(IValueConverter listConverter, IValueConverter endConverter)
        {
            _listConverter = listConverter;
            _endConverter = endConverter;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var items = value.ToList();
            var n = items.Count - 1;
            var values = new IPropertyValue[n + 1];

            for (var i = 0; i < n; i++)
            {
                values[i] = _listConverter.Convert(items[i]);

                if (values[i] == null) return null;
            }

            values[n] = _endConverter.Convert(items[n]);
            return values[n] != null ? new ListValue(values, value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var valueList = new List<List<Token>>[properties.Length];
            var dummies = new Property[properties.Length];
            var max = 0;

            for (var i = 0; i < properties.Length; i++)
            {
                var value = properties[i].DeclaredValue;
                valueList[i] = value != null ? value.Original.ToList() : new List<List<Token>>();

                dummies[i] = PropertyFactory.Instance.CreateLonghand(properties[i].Name);
                max = Math.Max(max, valueList[i].Count);
            }

            var values = new IPropertyValue[max];

            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < dummies.Length; j++)
                {
                    var list = valueList[j];
                    var tokens = list.Count > i ? list[i] : Enumerable.Empty<Token>();
                    dummies[j].TrySetValue(new TokenValue(tokens));
                }

                var converter = i < max - 1 ? _listConverter : _endConverter;
                values[i] = converter.Construct(dummies);
            }

            return new ListValue(values, Enumerable.Empty<Token>());
        }

        private sealed class ListValue : IPropertyValue
        {
            private readonly IPropertyValue[] _values;

            public ListValue(IPropertyValue[] values, IEnumerable<Token> tokens)
            {
                _values = values;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get { return string.Join(", ", _values.Select(m => m.CssText)); }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                var tokens = new List<Token>();

                foreach (var value in _values)
                {
                    var extracted = value.ExtractFor(name);

                    if (extracted == null) continue;

                    if (tokens.Count > 0) tokens.Add(Token.Comma);

                    tokens.AddRange(extracted);
                }

                return new TokenValue(tokens);
            }
        }
    }
}