namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollBehaviorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ScrollConverters.ScrollBehaviorConverter.OrDefault();

        internal ScrollBehaviorProperty()
            : base(PropertyNames.ScrollBehavior)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
