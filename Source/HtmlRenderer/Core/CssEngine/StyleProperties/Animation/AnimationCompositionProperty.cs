namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AnimationCompositionProperty : Property
    {
        private static readonly IValueConverter ListConverter = TimelineConverters.AnimationComposition.FromList().OrDefault();

        internal AnimationCompositionProperty()
            : base(PropertyNames.AnimationComposition)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}