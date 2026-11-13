namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontVariantNumericProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new FontGrammarValueConverter(FontVariantGrammar.TryParseNumeric).OrGlobalValue();

        internal FontVariantNumericProperty()
            : base(PropertyNames.FontVariantNumeric, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
