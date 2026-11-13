namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CaptionSideProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.CaptionSideConverter.OrDefault(true);

        internal CaptionSideProperty() : base(PropertyNames.CaptionSide, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}