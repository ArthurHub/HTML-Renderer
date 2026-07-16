namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PageBreakInsideProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.Assign(Keywords.Auto, BreakMode.Auto)
                .Or(Keywords.Avoid, BreakMode.Avoid)
                .OrDefault(BreakMode.Auto);

        internal PageBreakInsideProperty()
            : base(PropertyNames.PageBreakInside)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}