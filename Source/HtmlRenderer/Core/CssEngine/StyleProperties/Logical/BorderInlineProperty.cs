namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderInlineProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option().For(PropertyNames.BorderInlineStartWidth, PropertyNames.BorderInlineEndWidth),
            LineStyleConverter.Option().For(PropertyNames.BorderInlineStartStyle, PropertyNames.BorderInlineEndStyle),
            CurrentColorConverter.Option().For(PropertyNames.BorderInlineStartColor, PropertyNames.BorderInlineEndColor)
        ).OrDefault();

        internal BorderInlineProperty()
            : base(PropertyNames.BorderInline, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
