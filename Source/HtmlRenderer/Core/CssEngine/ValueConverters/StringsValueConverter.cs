using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StringsValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var items = value.ToItems();
            var n = items.Count;

            if (n % 2 != 0) return null;

            var values = new string[items.Count];

            for (var i = 0; i < n; i++)
            {
                values[i] = items[i].ToCssString();

                if (values[i] == null) return null;
            }

            return new StringsValue(values, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<StringsValue>();
        }

        private sealed class StringsValue : IPropertyValue
        {
            private readonly string[] _values;

            public StringsValue(string[] values, IEnumerable<Token> tokens)
            {
                _values = values;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get { return string.Join(" ", _values.Select(m => m.StylesheetString())); }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }
    }
}