namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollPaddingBottomProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.PaddingConverter.OrDefault();

        internal ScrollPaddingBottomProperty()
            : base(PropertyNames.ScrollPaddingBottom, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
