namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StrokeOpacityProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.NumberConverter.OrDefault(1f);

        internal StrokeOpacityProperty()
            : base(PropertyNames.StrokeOpacity, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}