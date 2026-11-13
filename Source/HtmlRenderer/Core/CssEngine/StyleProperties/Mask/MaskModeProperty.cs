namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MaskModeProperty : Property
    {
        private static readonly IValueConverter StyleConverter = MaskConverters.ModeConverter.OrDefault();

        internal MaskModeProperty()
            : base(PropertyNames.MaskMode)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
