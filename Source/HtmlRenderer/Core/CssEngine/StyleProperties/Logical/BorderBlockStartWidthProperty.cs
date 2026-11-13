namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBlockStartWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LineWidthConverter.OrDefault(Length.Medium);

        internal BorderBlockStartWidthProperty()
            : base(PropertyNames.BorderBlockStartWidth, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
