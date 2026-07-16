namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OutlineColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.InvertedColorConverter.OrDefault(Color.Transparent);

        internal OutlineColorProperty()
            : base(PropertyNames.OutlineColor, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}