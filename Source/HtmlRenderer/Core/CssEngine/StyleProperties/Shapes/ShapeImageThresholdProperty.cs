namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ShapeImageThresholdProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new NonNegativeValueConverter(Converters.PercentOrNumberConverter).OrDefault();

        internal ShapeImageThresholdProperty()
            : base(PropertyNames.ShapeImageThreshold, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
