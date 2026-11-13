namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class ScrollTimelineProperty : ShorthandProperty
    {
        private static readonly IValueConverter ListConverter = WithOrder(
            TimelineConverters.TimelineName.Required().For(PropertyNames.ScrollTimelineName),
            TimelineConverters.TimelineAxis.Option().For(PropertyNames.ScrollTimelineAxis)).FromList().OrDefault();

        internal ScrollTimelineProperty()
            : base(PropertyNames.ScrollTimeline)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}