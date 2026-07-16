namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ColumnCountProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.OptionalIntegerConverter.OrDefault();

        internal ColumnCountProperty()
            : base(PropertyNames.ColumnCount, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}