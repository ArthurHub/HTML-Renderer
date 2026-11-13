namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class AnimationRangeProperty : ShorthandProperty
    {
        private static readonly IValueConverter ListConverter = new AnimationRangeValueConverter().FromList().OrDefault();

        internal AnimationRangeProperty()
            : base(PropertyNames.AnimationRange)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}