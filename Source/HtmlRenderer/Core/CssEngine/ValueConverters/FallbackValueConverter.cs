using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FallbackValueConverter : IValueConverter
    {
        private readonly IValueConverter _converter;
        private readonly TokenValue _defaultValue;

        public FallbackValueConverter(IValueConverter converter, TokenValue defaultValue)
        {
            _converter = converter;
            _defaultValue = defaultValue;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            return value.Any() ? _converter.Convert(value) : new OptionValue(_defaultValue);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return _converter.Construct(properties) ?? new OptionValue(_defaultValue);
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
                return Original;
            }
        }
    }
}
