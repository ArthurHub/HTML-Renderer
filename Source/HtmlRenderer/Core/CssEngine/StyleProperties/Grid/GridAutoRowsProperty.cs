namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>grid-auto-rows</c> property (CSS Grid §7.6): the size of implicitly-created rows.</summary>
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
