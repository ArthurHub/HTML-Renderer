namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBlockStartColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CurrentColorConverter.OrDefault(Color.Transparent);

        internal BorderBlockStartColorProperty()
            : base(PropertyNames.BorderBlockStartColor)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
