namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PointerEventsProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("auto", "none", "visiblePainted", "visibleFill", "visibleStroke", "visible", "painted", "fill", "stroke", "all").OrDefault();

        internal PointerEventsProperty()
            : base(PropertyNames.PointerEvents, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
