namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextAnchorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.TextAnchorConverter;

        public TextAnchorProperty()
            : base(PropertyNames.TextAnchor)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}