namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ZIndexProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.OptionalIntegerConverter.OrDefault();

        internal ZIndexProperty()
            : base(PropertyNames.ZIndex, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}