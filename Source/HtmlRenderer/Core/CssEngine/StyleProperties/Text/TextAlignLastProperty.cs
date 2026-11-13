namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextAlignLastProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.TextAlignLastConverter.OrDefault();

        public TextAlignLastProperty()
            : base(PropertyNames.TextAlignLast, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}