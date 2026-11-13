namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollPaddingLeftProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.PaddingConverter.OrDefault();

        internal ScrollPaddingLeftProperty()
            : base(PropertyNames.ScrollPaddingLeft, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
