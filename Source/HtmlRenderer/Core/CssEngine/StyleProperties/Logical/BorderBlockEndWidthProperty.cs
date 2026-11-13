namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBlockEndWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LineWidthConverter.OrDefault(Length.Medium);

        internal BorderBlockEndWidthProperty()
            : base(PropertyNames.BorderBlockEndWidth, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
