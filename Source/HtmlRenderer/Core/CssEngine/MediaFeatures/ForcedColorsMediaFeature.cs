namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ForcedColorsMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter = Converters.Toggle(Keywords.None, Keywords.Active);

        public ForcedColorsMediaFeature() : base(FeatureNames.ForcedColors)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}
