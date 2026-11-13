using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Wraps a single-token numeric converter and rejects a negative literal, for the grammars written
    /// <c>&lt;length-percentage [0,∞]&gt;</c> or <c>&lt;number [0,∞]&gt;</c>.
    /// </summary>
    internal sealed class NonNegativeValueConverter : IValueConverter
    {
        private readonly IValueConverter _converter;

        public NonNegativeValueConverter(IValueConverter converter)
        {
            _converter = converter;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.ToArray();

            foreach (var token in tokens)
            {
                if (token is UnitToken unit && unit.Value < 0f) return null;
                if (token is NumberToken number && number.Value < 0f) return null;
            }

            return _converter.Convert(tokens);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return _converter.Construct(properties);
        }
    }
}
