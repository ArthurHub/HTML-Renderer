namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderImageSourceProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.OptionalImageSourceConverter.OrDefault();

        internal BorderImageSourceProperty()
            : base(PropertyNames.BorderImageSource)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}