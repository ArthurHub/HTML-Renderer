namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBlockStyleProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.LineStyleConverter.Periodic(
                PropertyNames.BorderBlockStartStyle, PropertyNames.BorderBlockEndStyle)
            .OrDefault();

        internal BorderBlockStyleProperty()
            : base(PropertyNames.BorderBlockStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
