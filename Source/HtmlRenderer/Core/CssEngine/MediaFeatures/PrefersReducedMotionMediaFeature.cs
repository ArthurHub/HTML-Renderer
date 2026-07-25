namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>prefers-reduced-motion</c> media feature (Media Queries 5). Matched against the renderer's
    /// fixed <c>reduce</c> stance (a static PDF has no motion) in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class PrefersReducedMotionMediaFeature : MediaFeature
    {
        public PrefersReducedMotionMediaFeature() : base(FeatureNames.PrefersReducedMotion)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
