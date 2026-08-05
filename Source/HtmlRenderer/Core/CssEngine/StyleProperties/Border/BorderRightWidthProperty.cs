namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderRightWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.OrDefault(Length.Medium);

        internal BorderRightWidthProperty()
            : base(PropertyNames.BorderRightWidth, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}