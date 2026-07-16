namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBottomRightRadiusProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.BorderRadiusConverter.OrDefault(Length.Zero);

        internal BorderBottomRightRadiusProperty()
            : base(PropertyNames.BorderBottomRightRadius, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}