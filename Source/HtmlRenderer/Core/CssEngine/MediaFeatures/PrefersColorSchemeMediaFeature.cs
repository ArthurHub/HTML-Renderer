namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Media Queries 5 §12.5, matched against the adapter's color scheme in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class PrefersColorSchemeMediaFeature : MediaFeature
    {
        public PrefersColorSchemeMediaFeature() : base(FeatureNames.PrefersColorScheme)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
