namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollPaddingTopProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.PaddingConverter.OrDefault();

        internal ScrollPaddingTopProperty()
            : base(PropertyNames.ScrollPaddingTop, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
