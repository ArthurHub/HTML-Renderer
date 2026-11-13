namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AnimationTimelineProperty : Property
    {
        private static readonly IValueConverter ListConverter = TimelineConverters.AnimationTimeline.FromList().OrDefault();

        internal AnimationTimelineProperty()
            : base(PropertyNames.AnimationTimeline)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}