namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBottomLeftRadiusProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.BorderRadiusConverter.OrDefault(Length.Zero);

        internal BorderBottomLeftRadiusProperty()
            : base(PropertyNames.BorderBottomLeftRadius, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}