namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollSnapTypeProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.ScrollSnapTypeConverter.OrDefault();

        internal ScrollSnapTypeProperty()
            : base(PropertyNames.ScrollSnapType)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
