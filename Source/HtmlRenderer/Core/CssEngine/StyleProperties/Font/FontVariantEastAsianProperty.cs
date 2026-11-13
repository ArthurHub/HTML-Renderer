namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontVariantEastAsianProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new FontGrammarValueConverter(FontVariantGrammar.TryParseEastAsian).OrGlobalValue();

        internal FontVariantEastAsianProperty()
            : base(PropertyNames.FontVariantEastAsian, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
