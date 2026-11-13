namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollPaddingRightProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.PaddingConverter.OrDefault();

        internal ScrollPaddingRightProperty()
            : base(PropertyNames.ScrollPaddingRight, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
