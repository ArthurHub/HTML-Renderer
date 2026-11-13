namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PrefersReducedDataMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter = Converters.Toggle(Keywords.NoPreference, Keywords.Reduce);

        public PrefersReducedDataMediaFeature() : base(FeatureNames.PrefersReducedData)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}
