namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class VideoDynamicRangeMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter = Converters.Toggle(Keywords.Standard, Keywords.High);

        public VideoDynamicRangeMediaFeature() : base(FeatureNames.VideoDynamicRange)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}
