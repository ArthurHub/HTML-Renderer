namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-column-start</c>, CSS Grid 2 §8.3.</summary>
    internal sealed class GridColumnStartProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.GridLineConverter.OrDefault();

        internal GridColumnStartProperty()
            : base(PropertyNames.GridColumnStart)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
