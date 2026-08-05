namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderLeftWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.OrDefault(Length.Medium);

        internal BorderLeftWidthProperty()
            : base(PropertyNames.BorderLeftWidth, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}