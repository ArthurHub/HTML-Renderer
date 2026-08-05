namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontVariantProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.FontVariantConverter.OrDefault(FontVariant.Normal);

        internal FontVariantProperty()
            : base(PropertyNames.FontVariant, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}