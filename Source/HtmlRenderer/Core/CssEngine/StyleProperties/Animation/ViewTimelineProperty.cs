namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class ViewTimelineProperty : ShorthandProperty
    {
        private static readonly IValueConverter ListConverter = WithOrder(
            TimelineConverters.TimelineName.Required().For(PropertyNames.ViewTimelineName),
            WithAny(
                TimelineConverters.TimelineAxis.Option().For(PropertyNames.ViewTimelineAxis),
                TimelineConverters.TimelineInset.Option().For(PropertyNames.ViewTimelineInset))).FromList().OrDefault();

        internal ViewTimelineProperty()
            : base(PropertyNames.ViewTimeline)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}