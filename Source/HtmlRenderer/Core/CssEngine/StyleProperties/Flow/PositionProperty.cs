namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PositionProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.PositionModeConverter.OrDefault(PositionMode.Static);

        internal PositionProperty()
            : base(PropertyNames.Position)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}