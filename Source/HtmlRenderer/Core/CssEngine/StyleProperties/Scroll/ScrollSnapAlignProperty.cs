namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollSnapAlignProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.ScrollSnapAlignConverter.OrDefault();

        internal ScrollSnapAlignProperty()
            : base(PropertyNames.ScrollSnapAlign)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
