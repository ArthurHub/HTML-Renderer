namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontSynthesisSmallCapsProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            FontGrammarValueConverter.ForKeywords(Keywords.Auto, Keywords.None).OrGlobalValue();

        internal FontSynthesisSmallCapsProperty()
            : base(PropertyNames.FontSynthesisSmallCaps, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
