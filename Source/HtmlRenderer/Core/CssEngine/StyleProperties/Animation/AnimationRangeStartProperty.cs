namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AnimationRangeStartProperty : Property
    {
        private static readonly IValueConverter ListConverter = TimelineConverters.RangeEdge.FromList().OrDefault();

        internal AnimationRangeStartProperty()
            : base(PropertyNames.AnimationRangeStart)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}