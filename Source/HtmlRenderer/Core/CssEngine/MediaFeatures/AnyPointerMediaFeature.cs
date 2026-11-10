namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Media Queries 5 §7.3, matched in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class AnyPointerMediaFeature : MediaFeature
    {
        public AnyPointerMediaFeature() : base(FeatureNames.AnyPointer)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
