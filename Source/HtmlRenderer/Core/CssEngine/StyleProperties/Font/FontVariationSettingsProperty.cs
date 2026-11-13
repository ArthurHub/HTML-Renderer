namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontVariationSettingsProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            FontFeatureTagListGrammar.VariationSettingsConverter.OrGlobalValue();

        internal FontVariationSettingsProperty()
            : base(PropertyNames.FontVariationSettings, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
