using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ContinuousValueConverter : IValueConverter
    {
        private readonly IValueConverter _converter;

        public ContinuousValueConverter(IValueConverter converter)
        {
            _converter = converter;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var list = new List<Token>(value);
            var options = new List<IPropertyValue>();

            if (list.Count <= 0) return null;

            while (list.Count != 0)
            {
                var option = _converter.VaryStart(list);

                if (option == null) return null;

                options.Add(option);
            }

            return new OptionsValue(options.ToArray(), value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<OptionsValue>();
        }

        private sealed class OptionsValue : IPropertyValue
        {
            private readonly IPropertyValue[] _options;

            public OptionsValue(IPropertyValue[] options, IEnumerable<Token> tokens)
            {
                _options = options;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get
                {
                    return string.Join(" ",
                        _options.Where(m => !string.IsNullOrEmpty(m.CssText)).Select(m => m.CssText));
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                var tokens = new List<Token>();

                foreach (var option in _options)
                {
                    var extracted = option.ExtractFor(name);

                    if (extracted == null) continue;

                    if (tokens.Count > 0) tokens.Add(Token.Whitespace);

                    tokens.AddRange(extracted);
                }

                return new TokenValue(tokens);
            }
        }
    }
}