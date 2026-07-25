namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBlockStyleProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.LineStyleConverter.Periodic(
            PropertyNames.BorderTopStyle, PropertyNames.BorderBottomStyle).OrDefault();

        internal BorderBlockStyleProperty()
            : base(PropertyNames.BorderBlockStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
