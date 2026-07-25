namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>prefers-contrast</c> media feature (Media Queries 5). Matched against the renderer's fixed
    /// <c>no-preference</c> stance in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class PrefersContrastMediaFeature : MediaFeature
    {
        public PrefersContrastMediaFeature() : base(FeatureNames.PrefersContrast)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
