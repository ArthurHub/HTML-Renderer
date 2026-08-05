namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>prefers-color-scheme</c> media feature (Media Queries 5). The value is validated as any
    /// identifier here; the actual <c>light</c>/<c>dark</c> match against the configured scheme happens
    /// in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class PrefersColorSchemeMediaFeature : MediaFeature
    {
        public PrefersColorSchemeMediaFeature() : base(FeatureNames.PrefersColorScheme)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
