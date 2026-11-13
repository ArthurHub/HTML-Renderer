namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OverflowXProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.OverflowModeConverter.OrDefault(Overflow.Visible);

        internal OverflowXProperty()
            : base(PropertyNames.OverflowX)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
