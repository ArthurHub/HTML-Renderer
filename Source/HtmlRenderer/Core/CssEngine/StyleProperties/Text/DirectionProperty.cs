namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class DirectionProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.DirectionModeConverter.OrDefault(DirectionMode.Ltr);

        internal DirectionProperty()
            : base(PropertyNames.Direction, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}