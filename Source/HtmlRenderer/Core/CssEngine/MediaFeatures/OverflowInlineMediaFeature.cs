namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OverflowInlineMediaFeature : MediaFeature
    {
        private static readonly IValueConverter TheConverter = Converters.Toggle(Keywords.None, Keywords.Scroll);

        public OverflowInlineMediaFeature() : base(FeatureNames.OverflowInline)
        {
        }

        internal override IValueConverter Converter => TheConverter;
    }
}
