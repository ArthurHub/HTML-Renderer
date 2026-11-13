namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollMarginRightProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.MarginConverter.OrDefault();

        internal ScrollMarginRightProperty()
            : base(PropertyNames.ScrollMarginRight, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
