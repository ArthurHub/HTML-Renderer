namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontSynthesisPositionProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            FontGrammarValueConverter.ForKeywords(Keywords.Auto, Keywords.None).OrGlobalValue();

        internal FontSynthesisPositionProperty()
            : base(PropertyNames.FontSynthesisPosition, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
