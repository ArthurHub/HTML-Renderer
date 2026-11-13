namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderInlineEndProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option().For(PropertyNames.BorderInlineEndWidth),
            LineStyleConverter.Option().For(PropertyNames.BorderInlineEndStyle),
            CurrentColorConverter.Option().For(PropertyNames.BorderInlineEndColor)
        ).OrDefault();

        internal BorderInlineEndProperty()
            : base(PropertyNames.BorderInlineEnd, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
