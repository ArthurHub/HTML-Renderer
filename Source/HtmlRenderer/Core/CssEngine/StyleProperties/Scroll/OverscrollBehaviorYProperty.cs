namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OverscrollBehaviorYProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.OverscrollBehaviorConverter.OrDefault();

        internal OverscrollBehaviorYProperty()
            : base(PropertyNames.OverscrollBehaviorY)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
