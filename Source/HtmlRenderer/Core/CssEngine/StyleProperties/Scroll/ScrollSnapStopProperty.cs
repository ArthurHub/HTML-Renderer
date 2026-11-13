namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollSnapStopProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.ScrollSnapStopConverter.OrDefault();

        internal ScrollSnapStopProperty()
            : base(PropertyNames.ScrollSnapStop)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
