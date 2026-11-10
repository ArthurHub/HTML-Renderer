namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-template-rows</c>, CSS Grid 2 §7.2.</summary>
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
