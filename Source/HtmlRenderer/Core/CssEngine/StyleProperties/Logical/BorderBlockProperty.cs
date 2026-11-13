namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderBlockProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option().For(PropertyNames.BorderBlockStartWidth, PropertyNames.BorderBlockEndWidth),
            LineStyleConverter.Option().For(PropertyNames.BorderBlockStartStyle, PropertyNames.BorderBlockEndStyle),
            CurrentColorConverter.Option().For(PropertyNames.BorderBlockStartColor, PropertyNames.BorderBlockEndColor)
        ).OrDefault();

        internal BorderBlockProperty()
            : base(PropertyNames.BorderBlock, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
