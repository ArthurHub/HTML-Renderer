namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-template-areas</c> property (CSS Grid §7.3). Validated by the shared
    /// <see cref="GridTemplateAreasGrammar"/>; the authored text is preserved.
    /// </summary>
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
