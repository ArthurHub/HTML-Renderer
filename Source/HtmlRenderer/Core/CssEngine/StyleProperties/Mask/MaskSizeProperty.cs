namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MaskSizeProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.BackgroundSizeConverter.FromList().OrDefault();

        internal MaskSizeProperty()
            : base(PropertyNames.MaskSize, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
