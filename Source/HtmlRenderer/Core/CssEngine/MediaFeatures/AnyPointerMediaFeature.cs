namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>any-pointer</c> media feature (Media Queries 4). Matched against the renderer's fixed
    /// <c>none</c> stance (a static PDF has no pointing device) in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class AnyPointerMediaFeature : MediaFeature
    {
        public AnyPointerMediaFeature() : base(FeatureNames.AnyPointer)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
