namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BufferedRenderingProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("auto", "dynamic", "static").OrDefault();

        internal BufferedRenderingProperty()
            : base(PropertyNames.BufferedRendering)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
