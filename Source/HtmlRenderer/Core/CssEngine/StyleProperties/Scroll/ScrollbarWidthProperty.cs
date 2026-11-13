namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollbarWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.ScrollbarWidthConverter.OrDefault();

        internal ScrollbarWidthProperty()
            : base(PropertyNames.ScrollbarWidth)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
