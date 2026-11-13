using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>Validates an <c>image-orientation</c> value through <see cref="ImageOrientationGrammar"/>; the authored text is preserved.</summary>
    internal sealed class ImageOrientationValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            return ImageOrientationGrammar.IsValid(value) ? new GrammarValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GrammarValue>();
        }
    }
}
