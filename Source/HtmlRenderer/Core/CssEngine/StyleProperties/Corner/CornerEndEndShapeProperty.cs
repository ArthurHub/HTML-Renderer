namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CornerEndEndShapeProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CornerShapeConverter.OrDefault(Keywords.Round);

        internal CornerEndEndShapeProperty()
            : base(PropertyNames.CornerEndEndShape)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
