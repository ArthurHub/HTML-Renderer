namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderBlockProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option()
                .For(PropertyNames.BorderTopWidth, PropertyNames.BorderBottomWidth),
            LineStyleConverter.Option()
                .For(PropertyNames.BorderTopStyle, PropertyNames.BorderBottomStyle),
            CurrentColorConverter.Option()
                .For(PropertyNames.BorderTopColor, PropertyNames.BorderBottomColor)
        ).OrDefault();

        internal BorderBlockProperty()
            : base(PropertyNames.BorderBlock, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
