using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>Validates an <c>image-resolution</c> value through <see cref="ImageResolutionGrammar"/>; the authored text is preserved.</summary>
    internal sealed class ImageResolutionValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            return ImageResolutionGrammar.IsValid(value) ? new GrammarValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GrammarValue>();
        }
    }
}
