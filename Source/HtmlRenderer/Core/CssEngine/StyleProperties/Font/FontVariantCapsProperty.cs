namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontVariantCapsProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.FontVariantConverter.Or(new FontGrammarValueConverter(FontVariantGrammar.TryParseCaps)).OrGlobalValue();

        internal FontVariantCapsProperty()
            : base(PropertyNames.FontVariantCaps, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
