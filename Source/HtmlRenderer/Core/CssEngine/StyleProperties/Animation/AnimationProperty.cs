namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class AnimationProperty : ShorthandProperty
    {
        private static readonly IValueConverter ListConverter = new TimeBasedShorthandConverter(
            0, // duration index
            2, // delay index  
            TimeConverter.Option().For(PropertyNames.AnimationDuration),           // 0
            TransitionConverter.Option().For(PropertyNames.AnimationTimingFunction), // 1
            TimeConverter.Option().For(PropertyNames.AnimationDelay),      // 2
            PositiveOrInfiniteNumberConverter.Option().For(PropertyNames.AnimationIterationCount), // 3
            AnimationDirectionConverter.Option().For(PropertyNames.AnimationDirection), // 4
            AnimationFillStyleConverter.Option().For(PropertyNames.AnimationFillMode), // 5
            PlayStateConverter.Option().For(PropertyNames.AnimationPlayState), // 6
            IdentifierConverter.Option().For(PropertyNames.AnimationName) // 7
        ).FromList().OrDefault();

        internal AnimationProperty() : base(PropertyNames.Animation)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}