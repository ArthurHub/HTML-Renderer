namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OverflowYProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.OverflowModeConverter.OrDefault(Overflow.Visible);

        internal OverflowYProperty()
            : base(PropertyNames.OverflowY)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
