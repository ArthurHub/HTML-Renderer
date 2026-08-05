namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FeatureProperty : Property
    {
        internal FeatureProperty(MediaFeature feature)
            : base(feature.Name)
        {
            Feature = feature;
        }


        internal override IValueConverter Converter => Feature.Converter;

        internal MediaFeature Feature { get; }
    }
}