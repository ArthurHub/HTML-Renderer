namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.ColorConverter.OrDefault(Color.Black);

        internal ColorProperty()
            : base(PropertyNames.Color, PropertyFlags.Inherited | PropertyFlags.Hashless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}