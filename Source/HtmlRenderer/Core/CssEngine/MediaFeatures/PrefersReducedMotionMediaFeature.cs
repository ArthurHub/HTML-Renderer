namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PrefersReducedMotionMediaFeature : MediaFeature
    {
        public PrefersReducedMotionMediaFeature() : base(FeatureNames.PrefersReducedMotion)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
