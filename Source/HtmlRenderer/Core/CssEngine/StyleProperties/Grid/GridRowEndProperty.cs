namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-row-end</c>, CSS Grid 2 §8.3.</summary>
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
