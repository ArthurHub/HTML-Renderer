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
            var value = _converter.Construct(properties);
            // A longhand this optional shorthand slot maps to may carry the literal "initial"
            // sentinel (see ShorthandProperty.Export) when the original shorthand text omitted it -
            // that's correct for cascade purposes, but re-serializing the shorthand must omit it
            // entirely (matching real browser serialization of e.g. "border: 1px outset" - the never-
            // specified color must not round-trip as "1px outset initial"), the same as if this slot
            // had never been set at all.
            return value is null || value.CssText == Keywords.Initial ? new OptionValue(Enumerable.Empty<Token>()) : value;
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
            var value = _converter.Construct(properties);
            // See OptionValueConverter.Construct above for why the "initial" sentinel must be
            // suppressed here rather than serialized literally.
            return value is null || value.CssText == Keywords.Initial ? new OptionValue(Enumerable.Empty<Token>()) : value;
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