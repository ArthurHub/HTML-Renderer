namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CornerBottomRightShapeProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CornerShapeConverter.OrDefault(Keywords.Round);

        internal CornerBottomRightShapeProperty()
            : base(PropertyNames.CornerBottomRightShape)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
