namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ShapeOutsideProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new ShapeOutsideValueConverter().OrDefault();

        internal ShapeOutsideProperty()
            : base(PropertyNames.ShapeOutside)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
