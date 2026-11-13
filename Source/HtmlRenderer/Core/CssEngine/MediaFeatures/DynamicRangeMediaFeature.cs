namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class DynamicRangeMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter = Converters.Toggle(Keywords.Standard, Keywords.High);

        public DynamicRangeMediaFeature() : base(FeatureNames.DynamicRange)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}
