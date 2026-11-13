namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ShapeRenderingProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("auto", "optimizeSpeed", "crispEdges", "geometricPrecision").OrDefault();

        internal ShapeRenderingProperty()
            : base(PropertyNames.ShapeRendering, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
