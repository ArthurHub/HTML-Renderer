namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderInlineStartWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LineWidthConverter.OrDefault(Length.Medium);

        internal BorderInlineStartWidthProperty()
            : base(PropertyNames.BorderInlineStartWidth, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
