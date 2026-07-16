namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderBottomProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option().For(PropertyNames.BorderBottomWidth),
            LineStyleConverter.Option().For(PropertyNames.BorderBottomStyle),
            CurrentColorConverter.Option().For(PropertyNames.BorderBottomColor)
        ).OrDefault();

        internal BorderBottomProperty()
            : base(PropertyNames.BorderBottom, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}