namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderInlineStyleProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.LineStyleConverter.Periodic(
                PropertyNames.BorderInlineStartStyle, PropertyNames.BorderInlineEndStyle)
            .OrDefault();

        internal BorderInlineStyleProperty()
            : base(PropertyNames.BorderInlineStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
