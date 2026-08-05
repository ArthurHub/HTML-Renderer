namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextAlignLastProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.TextAlignLastConverter;

        public TextAlignLastProperty()
            : base(PropertyNames.TextAlignLast)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}