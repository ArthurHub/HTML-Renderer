namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class WordSpacingProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.OptionalLengthConverter.OrDefault();

        internal WordSpacingProperty()
            : base(
                PropertyNames.WordSpacing, PropertyFlags.Inherited | PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}