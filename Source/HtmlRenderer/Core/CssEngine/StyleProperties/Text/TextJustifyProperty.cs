namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextJustifyProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.TextJustifyConverter;

        public TextJustifyProperty()
            : base(PropertyNames.TextJustify, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}