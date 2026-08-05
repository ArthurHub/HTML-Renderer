namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class LetterSpacingProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.OptionalLengthConverter.OrDefault();

        internal LetterSpacingProperty()
            : base(PropertyNames.LetterSpacing, PropertyFlags.Inherited | PropertyFlags.Unitless)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}