using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StringValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var str = value.ToCssString();
            return str != null ? new StringValue(str, value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<StringValue>();
        }

        private sealed class StringValue : IPropertyValue
        {
            private readonly string _value;

            public StringValue(string value, IEnumerable<Token> tokens)
            {
                _value = value;
                Original = new TokenValue(tokens);
            }

            public string CssText => _value.StylesheetString();

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }
    }
}