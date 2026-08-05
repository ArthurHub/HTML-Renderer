namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class BorderLeftProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            LineWidthConverter.Option().For(PropertyNames.BorderLeftWidth),
            LineStyleConverter.Option().For(PropertyNames.BorderLeftStyle),
            CurrentColorConverter.Option().For(PropertyNames.BorderLeftColor)
        ).OrDefault();

        internal BorderLeftProperty()
            : base(PropertyNames.BorderLeft, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}