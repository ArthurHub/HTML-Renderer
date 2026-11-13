namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MaskClipProperty : Property
    {
        private static readonly IValueConverter StyleConverter = MaskConverters.ClipConverter.OrDefault();

        internal MaskClipProperty()
            : base(PropertyNames.MaskClip)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
