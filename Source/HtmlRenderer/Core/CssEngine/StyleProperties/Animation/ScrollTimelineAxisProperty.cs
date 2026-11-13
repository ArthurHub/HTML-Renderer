namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScrollTimelineAxisProperty : Property
    {
        private static readonly IValueConverter ListConverter = TimelineConverters.TimelineAxis.FromList().OrDefault();

        internal ScrollTimelineAxisProperty()
            : base(PropertyNames.ScrollTimelineAxis)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}