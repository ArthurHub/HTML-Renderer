namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Media Queries 5 §12.2, matched in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class PrefersReducedTransparencyMediaFeature : MediaFeature
    {
        public PrefersReducedTransparencyMediaFeature() : base(FeatureNames.PrefersReducedTransparency)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
