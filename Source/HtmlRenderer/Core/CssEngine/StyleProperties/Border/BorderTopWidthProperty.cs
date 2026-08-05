namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderTopWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.OrDefault(Length.Medium);

        internal BorderTopWidthProperty()
            : base(PropertyNames.BorderTopWidth, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}