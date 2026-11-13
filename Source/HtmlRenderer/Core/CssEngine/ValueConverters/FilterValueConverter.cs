using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>Validates a <c>filter</c> / <c>backdrop-filter</c> value through <see cref="FilterGrammar"/>; the authored text is preserved.</summary>
    internal sealed class FilterValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            return FilterGrammar.TryParse(value) != null ? new GrammarValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GrammarValue>();
        }
    }
}
