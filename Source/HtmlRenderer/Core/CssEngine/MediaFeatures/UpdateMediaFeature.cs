namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Media Queries 5 §5.4, matched in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class UpdateMediaFeature : MediaFeature
    {
        public UpdateMediaFeature() : base(FeatureNames.Update)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
