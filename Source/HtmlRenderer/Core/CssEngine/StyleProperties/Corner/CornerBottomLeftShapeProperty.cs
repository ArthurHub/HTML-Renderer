namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CornerBottomLeftShapeProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CornerShapeConverter.OrDefault(Keywords.Round);

        internal CornerBottomLeftShapeProperty()
            : base(PropertyNames.CornerBottomLeftShape)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
