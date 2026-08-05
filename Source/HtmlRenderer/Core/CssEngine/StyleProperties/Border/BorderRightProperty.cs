namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderRightProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option().For(PropertyNames.BorderRightWidth),
            LineStyleConverter.Option().For(PropertyNames.BorderRightStyle),
            CurrentColorConverter.Option().For(PropertyNames.BorderRightColor)
        ).OrDefault();

        internal BorderRightProperty()
            : base(PropertyNames.BorderRight, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}