using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StartsWithValueConverter : IValueConverter
    {
        private readonly IValueConverter _converter;
        private readonly string _data;
        private readonly TokenType _type;

        public StartsWithValueConverter(TokenType type, string data, IValueConverter converter)
        {
            _type = type;
            _data = data;
            _converter = converter;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var rest = Transform(value);
            return rest != null ? CreateFrom(_converter.Convert(rest), value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var value = _converter.Construct(properties);
            return value != null ? CreateFrom(value, Enumerable.Empty<Token>()) : null;
        }

        private IPropertyValue CreateFrom(IPropertyValue value, IEnumerable<Token> tokens)
        {
            return value != null ? new StartValue(_data, value, tokens) : null;
        }

        private List<Token> Transform(IEnumerable<Token> values)
        {
            using (var enumerator = values.GetEnumerator())
            {
                bool hasCurrent;
                while ((hasCurrent = enumerator.MoveNext()) && enumerator.Current.Type == TokenType.Whitespace)
                {
                    //Empty on purpose.
                }

                // An empty (or whitespace-only) input - e.g. ConvertDefault()'s Enumerable.Empty<Token>() -
                // must resolve to "no match", not throw, exactly like every other converter's empty-input
                // behavior (e.g. DictionaryValueConverter's ToIdentifier() returning null).
                if (!hasCurrent) return null;

                if (enumerator.Current.Type != _type || !enumerator.Current.Data.Isi(_data)) return null;
                var list = new List<Token>();

                while (enumerator.MoveNext())
                    if (enumerator.Current.Type != TokenType.Whitespace || list.Count != 0)
                        list.Add(enumerator.Current);

                return list;
            }
        }

        private sealed class StartValue : IPropertyValue
        {
            private readonly string _start;
            private readonly IPropertyValue _value;

            public StartValue(string start, IPropertyValue value, IEnumerable<Token> tokens)
            {
                _start = start;
                _value = value;
                Original = new TokenValue(tokens);
            }

            public string CssText => string.Concat(_start, " ", _value.CssText);

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return _value.ExtractFor(name);
            }
        }
    }
}