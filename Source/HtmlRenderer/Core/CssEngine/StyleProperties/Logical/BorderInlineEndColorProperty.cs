namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderInlineEndColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CurrentColorConverter.OrDefault(Color.Transparent);

        internal BorderInlineEndColorProperty()
            : base(PropertyNames.BorderInlineEndColor)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
