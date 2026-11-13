namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderBlockEndProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option().For(PropertyNames.BorderBlockEndWidth),
            LineStyleConverter.Option().For(PropertyNames.BorderBlockEndStyle),
            CurrentColorConverter.Option().For(PropertyNames.BorderBlockEndColor)
        ).OrDefault();

        internal BorderBlockEndProperty()
            : base(PropertyNames.BorderBlockEnd, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
