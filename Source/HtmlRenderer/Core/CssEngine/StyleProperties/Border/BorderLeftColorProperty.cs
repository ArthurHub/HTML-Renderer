namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderLeftColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CurrentColorConverter.OrDefault(Color.Transparent);

        internal BorderLeftColorProperty()
            : base(PropertyNames.BorderLeftColor)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}