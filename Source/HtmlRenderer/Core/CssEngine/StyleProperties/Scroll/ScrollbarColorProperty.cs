namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollbarColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.ScrollbarColorConverter.OrDefault();

        internal ScrollbarColorProperty()
            : base(PropertyNames.ScrollbarColor, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
