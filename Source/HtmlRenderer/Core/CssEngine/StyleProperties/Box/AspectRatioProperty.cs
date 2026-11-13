namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
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
