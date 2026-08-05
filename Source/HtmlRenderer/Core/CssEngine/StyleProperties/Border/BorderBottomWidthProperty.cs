namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBottomWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.OrDefault(Length.Medium);

        internal BorderBottomWidthProperty()
            : base(PropertyNames.BorderBottomWidth, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}