namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontLanguageOverrideProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            FontLanguageOverrideGrammar.Converter.OrGlobalValue();

        internal FontLanguageOverrideProperty()
            : base(PropertyNames.FontLanguageOverride, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
