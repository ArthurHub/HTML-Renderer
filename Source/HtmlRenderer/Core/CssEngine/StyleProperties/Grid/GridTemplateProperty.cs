namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-template</c> shorthand (CSS Grid §7.4): expands to <c>grid-template-rows</c> /
    /// <c>grid-template-columns</c> / <c>grid-template-areas</c>.
    /// </summary>
    internal sealed class GridTemplateProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.GridTemplateConverter.OrGlobalValue();

        internal GridTemplateProperty()
            : base(PropertyNames.GridTemplate)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
