using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates an <c>aspect-ratio</c> value through the shared <see cref="AspectRatioGrammar"/> — accepting
    /// <c>auto</c>, a <c>&lt;ratio&gt;</c>, or <c>auto &amp;&amp; &lt;ratio&gt;</c>, and rejecting everything
    /// else so an invalid value is dropped at parse time. The authored text is preserved so Layer B
    /// (<see cref="Html.Core.Dom.CssLayoutEngine"/>) re-parses the same value during layout.
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
