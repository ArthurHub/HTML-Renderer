using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class UrlValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var str = value.ToUri();
            return str != null ? new UrlValue(str, value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<UrlValue>();
        }

        private sealed class UrlValue : IPropertyValue
        {
            private readonly string _value;

            public UrlValue(string value, IEnumerable<Token> tokens)
            {
                _value = value;
                Original = new TokenValue(tokens);
            }

            public string CssText => _value.StylesheetUrl();

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }
    }
}