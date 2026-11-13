namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ImageOrientationProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new ImageOrientationValueConverter().OrDefault();

        internal ImageOrientationProperty()
            : base(PropertyNames.ImageOrientation, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
