using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates <c>color-scheme</c> (<c>normal | [ light | dark | &lt;custom-ident&gt; ]+ &amp;&amp; only?</c>)
    /// through <see cref="ColorSchemeGrammar"/>. The authored text is preserved.
    /// </summary>
    internal sealed class ColorSchemeValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            return ColorSchemeGrammar.TryParse(tokens) ? new ColorSchemeValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<ColorSchemeValue>();
        }

        private sealed class ColorSchemeValue : IPropertyValue
        {
            public ColorSchemeValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
