using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates a <c>box-shadow</c> value through the shared <see cref="BoxShadowGrammar"/> - accepting
    /// <c>none</c> or a comma-separated list of <c>[ inset? &amp;&amp; &lt;length&gt;{2,4} &amp;&amp;
    /// &lt;color&gt;? ]</c> shadow layers, and rejecting everything else so an invalid value (e.g.
    /// <c>box-shadow: banana</c>) is dropped at parse time. The authored value text is preserved verbatim
    /// so the paint-time resolver (Layer B, <c>CssBox.PaintBoxShadows</c>) re-tokenizes and resolves the
    /// same value against the box - a single grammar shared across both layers.
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

            // Preserve the authored text verbatim - Layer B re-parses and resolves this string.
            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
