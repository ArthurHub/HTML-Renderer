namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MaskOriginProperty : Property
    {
        private static readonly IValueConverter StyleConverter = MaskConverters.OriginConverter.OrDefault();

        internal MaskOriginProperty()
            : base(PropertyNames.MaskOrigin)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
