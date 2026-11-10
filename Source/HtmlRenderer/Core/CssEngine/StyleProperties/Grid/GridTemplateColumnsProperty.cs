namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-template-columns</c>, CSS Grid 2 §7.2.</summary>
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
