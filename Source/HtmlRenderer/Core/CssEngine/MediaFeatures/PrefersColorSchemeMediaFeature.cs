namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PrefersColorSchemeMediaFeature : MediaFeature
    {
        public PrefersColorSchemeMediaFeature() : base(FeatureNames.PrefersColorScheme)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
