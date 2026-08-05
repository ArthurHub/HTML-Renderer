namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderRadiusProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.BorderRadiusShorthandConverter.OrDefault();

        internal BorderRadiusProperty()
            : base(PropertyNames.BorderRadius, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}