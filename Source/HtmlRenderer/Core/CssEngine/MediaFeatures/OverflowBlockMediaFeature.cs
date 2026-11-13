namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OverflowBlockMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter = Converters.Toggle(Keywords.None, Keywords.Scroll).Or(Keywords.Paged);

        public OverflowBlockMediaFeature() : base(FeatureNames.OverflowBlock)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}
