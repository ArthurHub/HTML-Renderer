namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontFeatureSettingsProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            FontFeatureTagListGrammar.FeatureSettingsConverter.OrGlobalValue();

        internal FontFeatureSettingsProperty()
            : base(PropertyNames.FontFeatureSettings, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
