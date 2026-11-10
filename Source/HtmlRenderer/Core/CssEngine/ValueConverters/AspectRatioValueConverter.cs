using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates an <c>aspect-ratio</c> value (CSS Sizing 4 §4.1) and keeps the authored text.
    /// </summary>
    internal sealed class AspectRatioValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.ToArray();
            return AspectRatioGrammar.TryParse(tokens, out _) ? new AspectRatioValue(tokens) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<AspectRatioValue>();
        }

        private sealed class AspectRatioValue : IPropertyValue
        {
            public AspectRatioValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
