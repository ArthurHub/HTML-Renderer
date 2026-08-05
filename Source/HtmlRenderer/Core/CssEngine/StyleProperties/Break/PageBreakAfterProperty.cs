namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PageBreakAfterProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.PageBreakModeConverter.OrDefault(BreakMode.Auto);

        internal PageBreakAfterProperty()
            : base(PropertyNames.PageBreakAfter)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}