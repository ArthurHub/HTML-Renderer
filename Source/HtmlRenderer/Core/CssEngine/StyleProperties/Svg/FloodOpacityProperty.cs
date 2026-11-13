namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FloodOpacityProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.PercentOrNumberConverter.OrDefault();

        internal FloodOpacityProperty()
            : base(PropertyNames.FloodOpacity, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
