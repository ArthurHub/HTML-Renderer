using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ListValueConverter : IValueConverter
    {
        private readonly IValueConverter _converter;

        public ListValueConverter(IValueConverter converter)
        {
            _converter = converter;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var items = value.ToList();
            var values = new IPropertyValue[items.Count];

            for (var i = 0; i < items.Count; i++)
            {
                values[i] = _converter.Convert(items[i]);

                if (values[i] == null) return null;
            }

            return values.Length != 1 ? new ListValue(values, value) : values[0];
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var result = properties.Guard<ListValue>();

            if (result == null)
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

                    values[i] = _converter.Construct(dummies);
                }

                result = new ListValue(values, Enumerable.Empty<Token>());
            }

            return result;
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

                    if (extracted != null)
                    {
                        if (tokens.Count > 0) tokens.Add(Token.Comma);

                        tokens.AddRange(extracted);
                    }
                }

                return new TokenValue(tokens);
            }
        }
    }
}