using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Wraps a shorthand's converter for a shorthand that also resets longhands it cannot express (the
    /// <c>font</c> shorthand resets <c>font-kerning</c>, <c>font-feature-settings</c>, the
    /// <c>font-variant-*</c> longhands, ... CSS Fonts 4 7.7). Parsing is unchanged - the declaration
    /// expansion resets any longhand the value does not mention to its initial value - but when
    /// serializing, the shorthand can only be rebuilt from the longhands while every such reset longhand
    /// still holds its initial value; otherwise the longhands would be lost.
    /// </summary>
    internal sealed class ImplicitResetValueConverter : IValueConverter
    {
        private readonly IValueConverter _converter;
        private readonly Dictionary<string, string[]> _initialValues;

        /// <param name="converter">The shorthand's own converter.</param>
        /// <param name="initialValues">For each implicitly reset longhand, the texts its initial value can
        /// take ("initial" is always accepted).</param>
        public ImplicitResetValueConverter(IValueConverter converter, Dictionary<string, string[]> initialValues)
        {
            _converter = converter;
            _initialValues = initialValues;
        }

        public IPropertyValue Convert(IEnumerable<Token> value) => _converter.Convert(value);

        public IPropertyValue Construct(Property[] properties)
        {
            foreach (var property in properties)
            {
                if (!_initialValues.TryGetValue(property.Name, out var accepted)) continue;

                var text = property.Value;

                if (!text.Is(Keywords.Initial) && !accepted.Any(a => a.Is(text))) return null;
            }

            return _converter.Construct(properties);
        }
    }
}
