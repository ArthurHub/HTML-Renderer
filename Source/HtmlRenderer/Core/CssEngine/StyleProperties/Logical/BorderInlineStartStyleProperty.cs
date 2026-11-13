namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderInlineStartStyleProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LineStyleConverter.OrDefault(LineStyle.None);

        internal BorderInlineStartStyleProperty()
            : base(PropertyNames.BorderInlineStartStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
