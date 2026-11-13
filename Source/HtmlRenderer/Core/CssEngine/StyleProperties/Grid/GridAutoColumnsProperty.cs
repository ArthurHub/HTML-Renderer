namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>grid-auto-columns</c> property (CSS Grid §7.6): the size of implicitly-created columns.</summary>
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
