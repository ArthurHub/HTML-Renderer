namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ResolutionMediaFeature : MediaFeature
    {
        public ResolutionMediaFeature(string name) : base(name)
        {
        }

        internal override IValueConverter Converter => Converters.ResolutionConverter;
    }
}