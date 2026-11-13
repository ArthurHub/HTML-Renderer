namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderBlockStartProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option().For(PropertyNames.BorderBlockStartWidth),
            LineStyleConverter.Option().For(PropertyNames.BorderBlockStartStyle),
            CurrentColorConverter.Option().For(PropertyNames.BorderBlockStartColor)
        ).OrDefault();

        internal BorderBlockStartProperty()
            : base(PropertyNames.BorderBlockStart, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
