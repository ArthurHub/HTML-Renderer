namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-column-end</c>, CSS Grid 2 §8.3.</summary>
    internal sealed class GridColumnEndProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.GridLineConverter.OrDefault();

        internal GridColumnEndProperty()
            : base(PropertyNames.GridColumnEnd)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
