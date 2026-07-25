namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-row-end</c> longhand (CSS Grid §8.3), one edge of a
    /// grid item's placement. Validated by the shared <see cref="GridLineGrammar"/>.
    /// </summary>
    internal sealed class GridRowEndProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.GridLineConverter.OrDefault();

        internal GridRowEndProperty()
            : base(PropertyNames.GridRowEnd)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
