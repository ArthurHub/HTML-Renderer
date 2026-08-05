namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class MonochromeMediaFeature : MediaFeature
    {
        public MonochromeMediaFeature(string name) : base(name)
        {
        }

        internal override IValueConverter Converter =>
            IsMinimum || IsMaximum ? NaturalIntegerConverter : NaturalIntegerConverter.Option(1);
    }
}