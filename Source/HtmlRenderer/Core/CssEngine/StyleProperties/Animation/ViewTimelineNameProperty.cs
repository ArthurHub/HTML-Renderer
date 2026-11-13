namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ViewTimelineNameProperty : Property
    {
        private static readonly IValueConverter ListConverter = TimelineConverters.TimelineName.FromList().OrDefault();

        internal ViewTimelineNameProperty()
            : base(PropertyNames.ViewTimelineName)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}