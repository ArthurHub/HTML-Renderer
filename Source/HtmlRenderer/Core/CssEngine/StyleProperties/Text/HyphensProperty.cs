namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class HyphensProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.HyphensConverter.OrDefault(Hyphens.Manual);

        internal HyphensProperty()
            : base(PropertyNames.Hyphens, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
