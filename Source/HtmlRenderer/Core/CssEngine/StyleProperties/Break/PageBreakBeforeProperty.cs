namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PageBreakBeforeProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.PageBreakModeConverter.OrDefault(BreakMode.Auto);

        internal PageBreakBeforeProperty()
            : base(PropertyNames.PageBreakBefore)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}