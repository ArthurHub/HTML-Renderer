using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ConditionalStartsWithValueConverter : IValueConverter
    {
        private readonly string _when;
        private readonly string[] _prefixKeywords;
        private readonly IValueConverter _converter;

        public ConditionalStartsWithValueConverter(string when, IValueConverter converter, params string[] prefixKeywords)
        {
            _when = when;
            _prefixKeywords = prefixKeywords;
            _converter = converter;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var enumerator = value.GetEnumerator();

            while (enumerator.MoveNext() && enumerator.Current.Type == TokenType.Whitespace)
            {
                //Empty on purpose.
            }

            if (enumerator.Current.Type != TokenType.Ident || !_prefixKeywords.Contains(enumerator.Current.Data))
                return null;

            var consumedPrefix = enumerator.Current.Data;

            var remainingTokens = new List<Token>();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Type == TokenType.Whitespace)
                    continue;

                remainingTokens.Add(enumerator.Current);
            }

            var result = _converter.Convert(remainingTokens);

            if (result != null && result.CssText != _when)
                return null;

            return new ConditionalStartValue(consumedPrefix, result, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var value = _converter.Construct(properties);
            return value != null ? CreateFrom(value, Enumerable.Empty<Token>()) : null;
        }

        private IPropertyValue CreateFrom(IPropertyValue value, IEnumerable<Token> tokens)
        {
            return value != null ? new ConditionalStartValue(string.Empty, value, tokens) : null;
        }

        private sealed class ConditionalStartValue : IPropertyValue
        {
            private readonly string _start;
            private readonly IPropertyValue _value;

            public ConditionalStartValue(string start, IPropertyValue value, IEnumerable<Token> tokens)
            {
                _start = start;
                _value = value;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get
                {
                    return !string.IsNullOrEmpty(_start)
                               ? string.Concat(_start, " ", _value.CssText)
                               : _value.CssText;
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return _value.ExtractFor(name);
            }
        }
    }
}
