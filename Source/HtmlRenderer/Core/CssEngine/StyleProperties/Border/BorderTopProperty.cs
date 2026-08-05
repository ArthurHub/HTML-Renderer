namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderTopProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option().For(PropertyNames.BorderTopWidth),
            LineStyleConverter.Option().For(PropertyNames.BorderTopStyle),
            CurrentColorConverter.Option().For(PropertyNames.BorderTopColor)
        ).OrDefault();

        internal BorderTopProperty()
            : base(PropertyNames.BorderTop, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}