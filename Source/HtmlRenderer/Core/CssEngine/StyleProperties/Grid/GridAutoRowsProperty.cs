namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-auto-rows</c>, CSS Grid 2 §7.6.</summary>
    internal sealed class GridAutoRowsProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new GridAutoTracksValueConverter().OrDefault();

        internal GridAutoRowsProperty()
            : base(PropertyNames.GridAutoRows)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
