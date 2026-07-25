namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-template-rows</c> property (CSS Grid Layout Module Level 1/2 §7.2). Validated by the
    /// shared <see cref="GridTrackListGrammar"/>; the authored text is preserved for the grid layout engine.
    /// </summary>
    internal sealed class GridTemplateRowsProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new GridTemplateValueConverter().OrDefault();

        internal GridTemplateRowsProperty()
            : base(PropertyNames.GridTemplateRows)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
