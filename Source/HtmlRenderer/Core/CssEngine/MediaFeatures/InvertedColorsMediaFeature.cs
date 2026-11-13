namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class InvertedColorsMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter = Converters.Toggle(Keywords.None, Keywords.Inverted);

        public InvertedColorsMediaFeature() : base(FeatureNames.InvertedColors)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}
