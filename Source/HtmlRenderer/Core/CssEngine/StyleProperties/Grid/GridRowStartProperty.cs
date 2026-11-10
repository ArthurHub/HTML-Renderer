namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-row-start</c>, CSS Grid 2 §8.3.</summary>
    internal sealed class GridRowStartProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.GridLineConverter.OrDefault();

        internal GridRowStartProperty()
            : base(PropertyNames.GridRowStart)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
