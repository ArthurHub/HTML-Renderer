namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderEndEndRadiusProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.BorderRadiusConverter.OrDefault(Length.Zero);

        internal BorderEndEndRadiusProperty()
            : base(PropertyNames.BorderEndEndRadius, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
