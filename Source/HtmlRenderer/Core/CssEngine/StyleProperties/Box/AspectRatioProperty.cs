namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// <c>aspect-ratio</c>, CSS Sizing 4 §4.1.
    /// </summary>
    internal sealed class AspectRatioProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new AspectRatioValueConverter().OrDefault();

        internal AspectRatioProperty()
            : base(PropertyNames.AspectRatio)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
