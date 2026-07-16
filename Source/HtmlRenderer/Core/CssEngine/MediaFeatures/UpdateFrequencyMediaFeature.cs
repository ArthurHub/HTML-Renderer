namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class UpdateFrequencyMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter = Map.UpdateFrequencies.ToConverter();

        public UpdateFrequencyMediaFeature() : base(FeatureNames.UpdateFrequency)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}