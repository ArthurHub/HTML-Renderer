namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextAnchorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.TextAnchorConverter;

        public TextAnchorProperty()
            : base(PropertyNames.TextAnchor, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}