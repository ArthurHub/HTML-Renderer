namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>any-hover</c> media feature (Media Queries 4). Matched against the renderer's fixed
    /// <c>none</c> stance (a static PDF has no hover-capable input) in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class AnyHoverMediaFeature : MediaFeature
    {
        public AnyHoverMediaFeature() : base(FeatureNames.AnyHover)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
