namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderTopLeftRadiusProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.BorderRadiusConverter.OrDefault(Length.Zero);

        internal BorderTopLeftRadiusProperty()
            : base(PropertyNames.BorderTopLeftRadius, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}