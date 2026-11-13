namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollMarginBottomProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.MarginConverter.OrDefault();

        internal ScrollMarginBottomProperty()
            : base(PropertyNames.ScrollMarginBottom, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
