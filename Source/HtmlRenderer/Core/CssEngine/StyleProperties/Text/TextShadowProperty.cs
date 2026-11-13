namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextShadowProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.TextShadowConverter.OrDefault();

        internal TextShadowProperty()
            : base(PropertyNames.TextShadow, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}