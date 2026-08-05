namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBlockWidthProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.Periodic(
            PropertyNames.BorderTopWidth, PropertyNames.BorderBottomWidth).OrDefault();

        internal BorderBlockWidthProperty()
            : base(PropertyNames.BorderBlockWidth, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
