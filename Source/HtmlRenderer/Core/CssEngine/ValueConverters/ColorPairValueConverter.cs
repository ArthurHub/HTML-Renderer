using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The arguments of <c>light-dark(&lt;color&gt;, &lt;color&gt;)</c> (CSS Color 5 section 4): exactly two comma-separated colors.
    /// A declaration whose arms are not both colors is invalid, as it is in a browser. A parser has no color scheme to pick an
    /// arm with, so the authored text is preserved and both arms are exposed through
    /// <see cref="ColorFunctionExtensions.TryGetLightDark"/> for the consumer that does.
    /// </summary>
    internal sealed class ColorPairValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value as Token[] ?? value.ToArray();
            if (!ColorFunctionExtensions.TrySplitColorPair(tokens, out var light, out var dark)) return null;

            // An arm is any color, light-dark() itself included (the nested form recurses through ColorConverter), and currentcolor.
            if (Converters.CurrentColorConverter.Convert(light) is null ||
                Converters.CurrentColorConverter.Convert(dark) is null)
                return null;

            return new PairValue(tokens);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<PairValue>();
        }

        private sealed class PairValue : IPropertyValue
        {
            public PairValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
