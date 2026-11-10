namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Media Queries 5 §7.3, matched in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class AnyHoverMediaFeature : MediaFeature
    {
        public AnyHoverMediaFeature() : base(FeatureNames.AnyHover)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
