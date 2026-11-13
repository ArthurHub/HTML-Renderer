namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class TransitionProperty : ShorthandProperty
    {
        // CSS Transitions 1 §2.5, none is only valid as a single transition
        internal static readonly IValueConverter ListConverter = SingleTransition(AnimatableConverter).FromList()
            .Or(SingleTransition(AnimatableConverter.OrNone()))
            .OrDefault();

        private static IValueConverter SingleTransition(IValueConverter propertyConverter)
        {
            return WithAny(
                propertyConverter.Option().For(PropertyNames.TransitionProperty),
                TimeConverter.Option().For(PropertyNames.TransitionDuration),
                TransitionConverter.Option().For(PropertyNames.TransitionTimingFunction),
                TimeConverter.Option().For(PropertyNames.TransitionDelay),
                TimelineConverters.TransitionBehavior.Option().For(PropertyNames.TransitionBehavior));
        }

        internal TransitionProperty()
            : base(PropertyNames.Transition)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}