namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CornerTopLeftShapeProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CornerShapeConverter.OrDefault(Keywords.Round);

        internal CornerTopLeftShapeProperty()
            : base(PropertyNames.CornerTopLeftShape)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
