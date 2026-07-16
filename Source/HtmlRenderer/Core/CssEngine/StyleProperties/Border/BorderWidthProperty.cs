namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderWidthProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.Periodic(
            PropertyNames.BorderTopWidth, PropertyNames.BorderRightWidth, PropertyNames.BorderBottomWidth,
            PropertyNames.BorderLeftWidth).OrDefault();

        internal BorderWidthProperty()
            : base(PropertyNames.BorderWidth, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}