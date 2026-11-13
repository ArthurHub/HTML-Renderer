namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ImageRenderingProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("auto", "smooth", "high-quality", "crisp-edges", "pixelated").OrDefault();

        internal ImageRenderingProperty()
            : base(PropertyNames.ImageRendering, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
