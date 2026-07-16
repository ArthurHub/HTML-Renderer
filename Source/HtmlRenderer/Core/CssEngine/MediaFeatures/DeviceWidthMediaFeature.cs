namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class DeviceWidthMediaFeature : MediaFeature
    {
        public DeviceWidthMediaFeature(string name) : base(name)
        {
        }

        internal override IValueConverter Converter => Converters.LengthConverter;
    }
}