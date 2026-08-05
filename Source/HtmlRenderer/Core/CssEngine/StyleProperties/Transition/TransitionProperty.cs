namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class TransitionProperty : ShorthandProperty
    {
        internal static readonly IValueConverter ListConverter = new TimeBasedShorthandConverter(
            1, // duration index
            3, // delay index
            AnimatableConverter.Option().For(PropertyNames.TransitionProperty), // 0
            TimeConverter.Option().For(PropertyNames.TransitionDuration), // 1
            TransitionConverter.Option().For(PropertyNames.TransitionTimingFunction), // 2
            TimeConverter.Option().For(PropertyNames.TransitionDelay) // 3
        ).FromList().OrDefault();

        internal TransitionProperty()
            : base(PropertyNames.Transition)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}