namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OverscrollBehaviorProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.OverscrollBehaviorConverter.Periodic(
                PropertyNames.OverscrollBehaviorX, PropertyNames.OverscrollBehaviorY)
            .OrDefault();

        internal OverscrollBehaviorProperty()
            : base(PropertyNames.OverscrollBehavior)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
