namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid</c> shorthand (CSS Grid §7.8): expands to the three <c>grid-template-*</c> longhands plus
    /// <c>grid-auto-flow</c> / <c>grid-auto-rows</c> / <c>grid-auto-columns</c>.
    /// </summary>
    internal sealed class GridProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.GridConverter.OrGlobalValue();

        internal GridProperty()
            : base(PropertyNames.Grid)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
