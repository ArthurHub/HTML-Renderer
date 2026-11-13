namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollPaddingProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.PaddingConverter.Periodic(
                PropertyNames.ScrollPaddingTop, PropertyNames.ScrollPaddingRight, PropertyNames.ScrollPaddingBottom,
                PropertyNames.ScrollPaddingLeft)
            .OrDefault(Length.Zero);

        internal ScrollPaddingProperty()
            : base(PropertyNames.ScrollPadding)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
