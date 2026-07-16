namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderTopRightRadiusProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.BorderRadiusConverter.OrDefault(Length.Zero);

        internal BorderTopRightRadiusProperty()
            : base(PropertyNames.BorderTopRightRadius, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}