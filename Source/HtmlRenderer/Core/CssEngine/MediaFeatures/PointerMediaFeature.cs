namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PointerMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter = Map.PointerAccuracies.ToConverter();

        public PointerMediaFeature() : base(FeatureNames.Pointer)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}