namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option()
                .For(PropertyNames.BorderTopWidth, PropertyNames.BorderRightWidth, PropertyNames.BorderBottomWidth,
                    PropertyNames.BorderLeftWidth),
            LineStyleConverter.Option()
                .For(PropertyNames.BorderTopStyle, PropertyNames.BorderRightStyle, PropertyNames.BorderBottomStyle,
                    PropertyNames.BorderLeftStyle),
            CurrentColorConverter.Option()
                .For(PropertyNames.BorderTopColor, PropertyNames.BorderRightColor, PropertyNames.BorderBottomColor,
                    PropertyNames.BorderLeftColor)
        ).OrDefault();

        internal BorderProperty()
            : base(PropertyNames.Border, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}