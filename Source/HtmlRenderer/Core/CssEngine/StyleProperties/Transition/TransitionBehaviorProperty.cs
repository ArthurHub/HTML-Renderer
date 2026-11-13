namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TransitionBehaviorProperty : Property
    {
        private static readonly IValueConverter ListConverter = TimelineConverters.TransitionBehavior.FromList().OrDefault();

        internal TransitionBehaviorProperty()
            : base(PropertyNames.TransitionBehavior)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}