namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ImageResolutionProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new ImageResolutionValueConverter().OrDefault();

        internal ImageResolutionProperty()
            : base(PropertyNames.ImageResolution, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
