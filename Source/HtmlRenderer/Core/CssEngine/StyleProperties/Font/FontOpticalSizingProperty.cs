namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontOpticalSizingProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            FontGrammarValueConverter.ForKeywords(Keywords.Auto, Keywords.None).OrGlobalValue();

        internal FontOpticalSizingProperty()
            : base(PropertyNames.FontOpticalSizing, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
