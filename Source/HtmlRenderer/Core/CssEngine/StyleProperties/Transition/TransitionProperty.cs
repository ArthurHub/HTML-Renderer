namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class TransitionProperty : ShorthandProperty
    {
        // css-transitions-1 §2.5: none is only valid when the list has a single item
        internal static readonly IValueConverter ListConverter = SingleTransition(AnimatableConverter).FromList()
            .Or(SingleTransition(AnimatableConverter.OrNone()))
            .OrDefault();

        internal TransitionProperty()
            : base(PropertyNames.Transition)
        {
        }

        internal override IValueConverter Converter => ListConverter;

        private static IValueConverter SingleTransition(IValueConverter propertyConverter)
        {
            return new TimeBasedShorthandConverter(
                1, // duration index
                3, // delay index
                propertyConverter.Option().For(PropertyNames.TransitionProperty), // 0
                TimeConverter.Option().For(PropertyNames.TransitionDuration), // 1
                TransitionConverter.Option().For(PropertyNames.TransitionTimingFunction), // 2
                TimeConverter.Option().For(PropertyNames.TransitionDelay) // 3
            );
        }
    }
}
