using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The parsed value shared by the GCPM / Generated Content function converters
    /// (<c>target-counter()</c>, <c>leader()</c>, <c>element()</c>, ...): it keeps the original tokens and
    /// serializes to the supplied canonical text.
    /// </summary>
    internal sealed class GcpmFunctionValue : IPropertyValue
    {
        public GcpmFunctionValue(string cssText, IEnumerable<Token> tokens)
        {
            CssText = cssText;
            Original = new TokenValue(tokens);
        }

        public string CssText { get; }

        public TokenValue Original { get; }

        public TokenValue ExtractFor(string name) => Original;
    }
}
