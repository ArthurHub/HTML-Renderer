using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class UnorderedOptionsConverter : IValueConverter
    {
        private readonly IValueConverter[] _converters;

        public UnorderedOptionsConverter(params IValueConverter[] converters)
        {
            _converters = converters;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var list = new List<Token>(value);
            var options = new IPropertyValue[_converters.Length];

            for (var i = 0; i < _converters.Length; i++)
            {
                options[i] = _converters[i].VaryAll(list);

                if (options[i] == null) return null;
            }

            return list.Count == 0 ? new OptionsValue(options, value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var result = properties.Guard<OptionsValue>();

            if (result != null) return result;

            var values = new IPropertyValue[_converters.Length];

            for (var i = 0; i < _converters.Length; i++)
            {
                var value = _converters[i].Construct(properties);

                if (value == null) return null;

                values[i] = value;
            }

            result = new OptionsValue(values, Enumerable.Empty<Token>());

            return result;
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

                    if (extracted != null && extracted.Count > 0)
                    {
                        if (tokens.Count > 0) tokens.Add(Token.Whitespace);

                        tokens.AddRange(extracted);
                    }
                }

                return new TokenValue(tokens);
            }
        }
    }
}
