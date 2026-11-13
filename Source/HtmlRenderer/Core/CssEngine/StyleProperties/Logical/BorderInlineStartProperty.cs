namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderInlineStartProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option().For(PropertyNames.BorderInlineStartWidth),
            LineStyleConverter.Option().For(PropertyNames.BorderInlineStartStyle),
            CurrentColorConverter.Option().For(PropertyNames.BorderInlineStartColor)
        ).OrDefault();

        internal BorderInlineStartProperty()
            : base(PropertyNames.BorderInlineStart, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
