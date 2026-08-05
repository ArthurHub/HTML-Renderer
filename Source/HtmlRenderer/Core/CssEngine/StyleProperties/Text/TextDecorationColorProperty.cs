namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextDecorationColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.ColorConverter.OrDefault(Color.Black);

        internal TextDecorationColorProperty()
            : base(PropertyNames.TextDecorationColor, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}