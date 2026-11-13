namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollMarginTopProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.MarginConverter.OrDefault();

        internal ScrollMarginTopProperty()
            : base(PropertyNames.ScrollMarginTop, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
