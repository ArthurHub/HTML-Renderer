namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MaskCompositeProperty : Property
    {
        private static readonly IValueConverter StyleConverter = MaskConverters.CompositeConverter.OrDefault();

        internal MaskCompositeProperty()
            : base(PropertyNames.MaskComposite)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
