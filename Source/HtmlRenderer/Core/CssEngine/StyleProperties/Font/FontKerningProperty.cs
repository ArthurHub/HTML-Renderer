namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontKerningProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            FontGrammarValueConverter.ForKeywords(Keywords.Auto, Keywords.Normal, Keywords.None).OrGlobalValue();

        internal FontKerningProperty()
            : base(PropertyNames.FontKerning, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
