namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>prefers-reduced-transparency</c> media feature (Media Queries 5). Matched against the
    /// renderer's fixed <c>no-preference</c> stance in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class PrefersReducedTransparencyMediaFeature : MediaFeature
    {
        public PrefersReducedTransparencyMediaFeature() : base(FeatureNames.PrefersReducedTransparency)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
