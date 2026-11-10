namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-auto-columns</c>, CSS Grid 2 §7.6.</summary>
    internal sealed class GridAutoColumnsProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new GridAutoTracksValueConverter().OrDefault();

        internal GridAutoColumnsProperty()
            : base(PropertyNames.GridAutoColumns)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
