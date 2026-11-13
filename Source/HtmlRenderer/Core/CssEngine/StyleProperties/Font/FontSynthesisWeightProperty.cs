namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontSynthesisWeightProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            FontGrammarValueConverter.ForKeywords(Keywords.Auto, Keywords.None).OrGlobalValue();

        internal FontSynthesisWeightProperty()
            : base(PropertyNames.FontSynthesisWeight, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
