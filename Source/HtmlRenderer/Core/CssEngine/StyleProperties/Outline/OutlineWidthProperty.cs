namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OutlineWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.OrDefault(Length.Medium);

        internal OutlineWidthProperty()
            : base(PropertyNames.OutlineWidth, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}