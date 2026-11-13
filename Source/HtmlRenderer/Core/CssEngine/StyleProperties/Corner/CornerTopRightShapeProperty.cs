namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CornerTopRightShapeProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CornerShapeConverter.OrDefault(Keywords.Round);

        internal CornerTopRightShapeProperty()
            : base(PropertyNames.CornerTopRightShape)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
