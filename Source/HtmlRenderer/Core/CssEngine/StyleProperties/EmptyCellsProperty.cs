namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class EmptyCellsProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.EmptyCellsConverter.OrDefault(true);

        internal EmptyCellsProperty()
            : base(PropertyNames.EmptyCells, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}