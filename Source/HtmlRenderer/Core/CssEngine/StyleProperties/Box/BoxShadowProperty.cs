namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BoxShadowProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.MultipleShadowConverter.OrDefault();

        internal BoxShadowProperty()
            : base(PropertyNames.BoxShadow, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}