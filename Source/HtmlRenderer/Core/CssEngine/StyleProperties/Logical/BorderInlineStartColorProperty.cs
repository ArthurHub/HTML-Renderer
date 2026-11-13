namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderInlineStartColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CurrentColorConverter.OrDefault(Color.Transparent);

        internal BorderInlineStartColorProperty()
            : base(PropertyNames.BorderInlineStartColor)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
