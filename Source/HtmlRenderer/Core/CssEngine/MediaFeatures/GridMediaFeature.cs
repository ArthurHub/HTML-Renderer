namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class GridMediaFeature : MediaFeature
    {
        public GridMediaFeature() : base(FeatureNames.Grid)
        {
        }

        internal override IValueConverter Converter => Converters.BinaryConverter;
    }
}