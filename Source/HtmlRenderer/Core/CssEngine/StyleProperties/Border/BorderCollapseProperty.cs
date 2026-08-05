namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderCollapseProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.BorderCollapseConverter.OrDefault(true);

        internal BorderCollapseProperty()
            : base(PropertyNames.BorderCollapse, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}