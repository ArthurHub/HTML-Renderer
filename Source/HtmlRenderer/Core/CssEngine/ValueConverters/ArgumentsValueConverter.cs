using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ArgumentsValueConverter : IValueConverter
    {
        private readonly IValueConverter[] _converters;

        public ArgumentsValueConverter(params IValueConverter[] converters)
        {
            _converters = converters;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var items = value.ToList();
            var length = _converters.Length;

            if (items.Count > length) return null;

            var args = new IPropertyValue[length];

            for (var i = 0; i < length; i++)
            {
                var item = i < items.Count ? items[i] : Enumerable.Empty<Token>();
                args[i] = _converters[i].Convert(item);

                if (args[i] == null) return null;
            }

            return new ArgumentsValue(args, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<ArgumentsValue>();
        }

        private sealed class ArgumentsValue : IPropertyValue
        {
            private readonly IPropertyValue[] _arguments;

            public ArgumentsValue(IPropertyValue[] arguments, IEnumerable<Token> tokens)
            {
                _arguments = arguments;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get
                {
                    var texts = _arguments.Where(m => !string.IsNullOrEmpty(m.CssText)).Select(m => m.CssText);
                    return string.Join(", ", texts);
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }
    }
}