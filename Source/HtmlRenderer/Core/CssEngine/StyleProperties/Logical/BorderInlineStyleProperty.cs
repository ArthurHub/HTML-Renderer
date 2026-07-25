namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderInlineStyleProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.LineStyleConverter.Periodic(
            PropertyNames.BorderLeftStyle, PropertyNames.BorderRightStyle).OrDefault();

        internal BorderInlineStyleProperty()
            : base(PropertyNames.BorderInlineStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
