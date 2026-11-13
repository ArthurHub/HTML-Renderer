namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderInlineWidthProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.Periodic(
                PropertyNames.BorderInlineStartWidth, PropertyNames.BorderInlineEndWidth)
            .OrDefault();

        internal BorderInlineWidthProperty()
            : base(PropertyNames.BorderInlineWidth, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
