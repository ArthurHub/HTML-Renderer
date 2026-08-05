namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-column-start</c> longhand (CSS Grid §8.3), one edge of a
    /// grid item's placement. Validated by the shared <see cref="GridLineGrammar"/>.
    /// </summary>
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
