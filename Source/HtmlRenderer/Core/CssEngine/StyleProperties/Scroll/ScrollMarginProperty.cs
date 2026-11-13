namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollMarginProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.MarginConverter.Periodic(
                PropertyNames.ScrollMarginTop, PropertyNames.ScrollMarginRight, PropertyNames.ScrollMarginBottom,
                PropertyNames.ScrollMarginLeft)
            .OrDefault(Length.Zero);

        internal ScrollMarginProperty()
            : base(PropertyNames.ScrollMargin)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
