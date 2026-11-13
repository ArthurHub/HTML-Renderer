namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ViewTimelineAxisProperty : Property
    {
        private static readonly IValueConverter ListConverter = TimelineConverters.TimelineAxis.FromList().OrDefault();

        internal ViewTimelineAxisProperty()
            : base(PropertyNames.ViewTimelineAxis)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}