namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CustomProperty : Property
    {
        internal CustomProperty(string name)
            : base(name)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
