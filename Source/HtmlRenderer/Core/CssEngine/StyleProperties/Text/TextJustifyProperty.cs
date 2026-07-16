namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextJustifyProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.TextJustifyConverter;

        public TextJustifyProperty()
            : base(PropertyNames.TextJustify)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}