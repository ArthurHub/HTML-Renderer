using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OptionValueConverter : IValueConverter
    {
        private readonly IValueConverter _converter;

        public OptionValueConverter(IValueConverter converter)
        {
            _converter = converter;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            return value.Any() ? _converter.Convert(value) : new OptionValue(value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return _converter.Construct(properties) ?? new OptionValue(Enumerable.Empty<Token>());
        }

        private sealed class OptionValue : IPropertyValue
        {
            public OptionValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => string.Empty;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return null;
            }
        }
    }

    internal sealed class OptionValueConverter<T> : IValueConverter
    {
        private readonly IValueConverter _converter;

        public OptionValueConverter(IValueConverter converter)
        {
            _converter = converter;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            return value.Any() ? _converter.Convert(value) : new OptionValue(value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return _converter.Construct(properties) ?? new OptionValue(Enumerable.Empty<Token>());
        }

        private sealed class OptionValue : IPropertyValue
        {
            public OptionValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => string.Empty;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return null;
            }
        }
    }
}