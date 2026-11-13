namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderInlineEndStyleProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LineStyleConverter.OrDefault(LineStyle.None);

        internal BorderInlineEndStyleProperty()
            : base(PropertyNames.BorderInlineEndStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
