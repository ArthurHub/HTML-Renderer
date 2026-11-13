namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FloodColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.OrDefault();

        internal FloodColorProperty()
            : base(PropertyNames.FloodColor, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
