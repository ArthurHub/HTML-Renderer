namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextRenderingProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("auto", "optimizeSpeed", "optimizeLegibility", "geometricPrecision").OrDefault();

        internal TextRenderingProperty()
            : base(PropertyNames.TextRendering, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
