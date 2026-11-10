namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Media Queries 5 §12.1, matched in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class PrefersReducedMotionMediaFeature : MediaFeature
    {
        public PrefersReducedMotionMediaFeature() : base(FeatureNames.PrefersReducedMotion)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
