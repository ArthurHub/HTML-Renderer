namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderInlineEndWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LineWidthConverter.OrDefault(Length.Medium);

        internal BorderInlineEndWidthProperty()
            : base(PropertyNames.BorderInlineEndWidth, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
