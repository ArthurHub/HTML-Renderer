namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class HoverMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter = Map.HoverAbilities.ToConverter();

        public HoverMediaFeature() : base(FeatureNames.Hover)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}