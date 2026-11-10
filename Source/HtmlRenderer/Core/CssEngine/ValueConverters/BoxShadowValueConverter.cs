using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates a <c>box-shadow</c> value with <see cref="BoxShadowGrammar"/> and keeps the authored text.
    /// </summary>
    internal sealed class BoxShadowValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.ToArray();
            return BoxShadowGrammar.TryParse(tokens) is null ? null : new BoxShadowValue(tokens);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<BoxShadowValue>();
        }

        private sealed class BoxShadowValue : IPropertyValue
        {
            public BoxShadowValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
