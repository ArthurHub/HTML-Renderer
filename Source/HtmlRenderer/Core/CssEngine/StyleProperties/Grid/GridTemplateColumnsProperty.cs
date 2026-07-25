namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-template-columns</c> property (CSS Grid Layout Module Level 1/2 §7.2). Validated by the
    /// shared <see cref="GridTrackListGrammar"/>; the authored text is preserved for the grid layout engine.
    /// </summary>
    internal sealed class GridTemplateColumnsProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new GridTemplateValueConverter().OrDefault();

        internal GridTemplateColumnsProperty()
            : base(PropertyNames.GridTemplateColumns)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
