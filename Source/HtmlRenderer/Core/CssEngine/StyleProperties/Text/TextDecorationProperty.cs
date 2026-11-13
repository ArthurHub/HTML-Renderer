namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class TextDecorationProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            ColorConverter.Option().For(PropertyNames.TextDecorationColor),
            TextDecorationStyleConverter.Option().For(PropertyNames.TextDecorationStyle),
            TextDecorationLinesConverter.Option().For(PropertyNames.TextDecorationLine),
            TextDecorationThicknessProperty.ValueConverter.Option().For(PropertyNames.TextDecorationThickness)).OrDefault();

        internal TextDecorationProperty()
            : base(PropertyNames.TextDecoration, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}