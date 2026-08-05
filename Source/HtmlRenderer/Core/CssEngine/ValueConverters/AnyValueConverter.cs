using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AnyValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            return new AnyValue(value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<AnyValue>();
        }

        private sealed class AnyValue : IPropertyValue
        {
            public AnyValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.ToText();

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }
    }
}