namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OrientationMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter = Converters.Toggle(Keywords.Portrait, Keywords.Landscape);

        public OrientationMediaFeature() : base(FeatureNames.Orientation)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}