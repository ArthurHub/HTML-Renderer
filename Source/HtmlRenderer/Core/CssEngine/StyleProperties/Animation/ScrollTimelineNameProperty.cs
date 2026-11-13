namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollTimelineNameProperty : Property
    {
        private static readonly IValueConverter ListConverter = TimelineConverters.TimelineName.FromList().OrDefault();

        internal ScrollTimelineNameProperty()
            : base(PropertyNames.ScrollTimelineName)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}