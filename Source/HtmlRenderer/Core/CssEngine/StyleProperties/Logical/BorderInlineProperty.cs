namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderInlineProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option()
                .For(PropertyNames.BorderLeftWidth, PropertyNames.BorderRightWidth),
            LineStyleConverter.Option()
                .For(PropertyNames.BorderLeftStyle, PropertyNames.BorderRightStyle),
            CurrentColorConverter.Option()
                .For(PropertyNames.BorderLeftColor, PropertyNames.BorderRightColor)
        ).OrDefault();

        internal BorderInlineProperty()
            : base(PropertyNames.BorderInline, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
