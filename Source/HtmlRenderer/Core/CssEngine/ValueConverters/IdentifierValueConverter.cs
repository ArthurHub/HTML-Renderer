using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class IdentifierValueConverter : IValueConverter
    {
        private readonly Func<IEnumerable<Token>, string> _converter;

        public IdentifierValueConverter(Func<IEnumerable<Token>, string> converter)
        {
            _converter = converter;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var result = _converter(value);
            return result != null ? new IdentifierValue(result, value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<IdentifierValue>();
        }

        private sealed class IdentifierValue : IPropertyValue
        {
            public IdentifierValue(string identifier, IEnumerable<Token> tokens)
            {
                CssText = identifier;
                Original = new TokenValue(tokens);
            }

            public string CssText { get; }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }
    }

    internal sealed class IdentifierValueConverter<T> : IValueConverter
    {
        private readonly string _identifier;
        private readonly T _result;

        public IdentifierValueConverter(string identifier, T result)
        {
            _identifier = identifier;
            _result = result;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            return value.Is(_identifier) ? new IdentifierValue(_identifier, value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<IdentifierValue>();
        }

        private sealed class IdentifierValue : IPropertyValue
        {
            public IdentifierValue(string identifier, IEnumerable<Token> tokens)
            {
                CssText = identifier;
                Original = new TokenValue(tokens);
            }

            public string CssText { get; }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }
    }
}