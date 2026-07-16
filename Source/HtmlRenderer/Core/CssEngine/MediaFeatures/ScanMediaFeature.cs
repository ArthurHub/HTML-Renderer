namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScanMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter =
            Converters.Toggle(Keywords.Interlace, Keywords.Progressive);

        public ScanMediaFeature() : base(FeatureNames.Scan)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}