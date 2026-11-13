namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontVariantAlternatesProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            FontVariantAlternatesGrammar.Converter.OrGlobalValue();

        internal FontVariantAlternatesProperty()
            : base(PropertyNames.FontVariantAlternates, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
