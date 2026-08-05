namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MaxWidthProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.OptionalLengthOrPercentConverter.OrDefault();

        internal MaxWidthProperty()
            : base(PropertyNames.MaxWidth, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}