namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AnimationRangeEndProperty : Property
    {
        private static readonly IValueConverter ListConverter = TimelineConverters.RangeEdge.FromList().OrDefault();

        internal AnimationRangeEndProperty()
            : base(PropertyNames.AnimationRangeEnd)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}