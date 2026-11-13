namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StopOpacityProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.PercentOrNumberConverter.OrDefault();

        internal StopOpacityProperty()
            : base(PropertyNames.StopOpacity, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
