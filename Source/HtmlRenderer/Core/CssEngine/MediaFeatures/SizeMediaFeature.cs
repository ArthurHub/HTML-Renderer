namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class SizeMediaFeature : MediaFeature
    {
        public SizeMediaFeature(string name) : base(name)
        {
        }

        internal override IValueConverter Converter => Converters.LengthConverter;
    }
}
