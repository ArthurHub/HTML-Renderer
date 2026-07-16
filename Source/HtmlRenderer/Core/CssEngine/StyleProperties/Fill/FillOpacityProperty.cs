namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FillOpacityProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.NumberConverter.OrDefault(1f);

        internal FillOpacityProperty()
            : base(PropertyNames.FillOpacity, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}