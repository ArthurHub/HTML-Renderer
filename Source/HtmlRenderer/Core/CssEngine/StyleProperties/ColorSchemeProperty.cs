namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ColorSchemeProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new ColorSchemeValueConverter().OrDefault();

        internal ColorSchemeProperty()
            : base(PropertyNames.ColorScheme, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
