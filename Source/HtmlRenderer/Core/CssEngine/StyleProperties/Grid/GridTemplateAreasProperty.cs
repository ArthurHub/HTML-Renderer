namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-template-areas</c>, CSS Grid 2 §7.3.</summary>
    internal sealed class GridTemplateAreasProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new GridTemplateAreasValueConverter().OrDefault();

        internal GridTemplateAreasProperty()
            : base(PropertyNames.GridTemplateAreas)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
