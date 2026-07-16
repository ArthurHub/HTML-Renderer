namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ListStylePositionProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.ListPositionConverter.OrDefault(ListPosition.Outside);

        internal ListStylePositionProperty()
            : base(PropertyNames.ListStylePosition, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}