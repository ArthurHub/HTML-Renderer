namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontSynthesisStyleProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            FontGrammarValueConverter.ForKeywords(Keywords.Auto, Keywords.None).OrGlobalValue();

        internal FontSynthesisStyleProperty()
            : base(PropertyNames.FontSynthesisStyle, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
