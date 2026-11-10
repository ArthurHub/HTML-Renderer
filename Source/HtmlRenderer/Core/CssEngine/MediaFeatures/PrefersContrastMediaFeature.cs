namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Media Queries 5 §12.3, matched in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class PrefersContrastMediaFeature : MediaFeature
    {
        public PrefersContrastMediaFeature() : base(FeatureNames.PrefersContrast)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
