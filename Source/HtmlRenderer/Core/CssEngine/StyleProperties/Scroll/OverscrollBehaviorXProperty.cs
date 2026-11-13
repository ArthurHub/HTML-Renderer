namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OverscrollBehaviorXProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.OverscrollBehaviorConverter.OrDefault();

        internal OverscrollBehaviorXProperty()
            : base(PropertyNames.OverscrollBehaviorX)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
