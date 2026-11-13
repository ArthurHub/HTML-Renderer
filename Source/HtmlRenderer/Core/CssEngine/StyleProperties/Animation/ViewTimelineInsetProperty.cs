namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ViewTimelineInsetProperty : Property
    {
        private static readonly IValueConverter ListConverter = TimelineConverters.TimelineInset.FromList().OrDefault();

        internal ViewTimelineInsetProperty()
            : base(PropertyNames.ViewTimelineInset)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}