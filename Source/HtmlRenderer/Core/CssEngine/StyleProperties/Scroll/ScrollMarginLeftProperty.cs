namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollMarginLeftProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.MarginConverter.OrDefault();

        internal ScrollMarginLeftProperty()
            : base(PropertyNames.ScrollMarginLeft, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
