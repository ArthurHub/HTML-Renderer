namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-column</c> shorthand (CSS Grid §8.3.1): <c>&lt;grid-line&gt; [ / &lt;grid-line&gt; ]?</c>,
    /// expanding to <c>grid-column-start</c> / <c>grid-column-end</c>.
    /// </summary>
    internal sealed class GridColumnProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.GridColumnConverter.OrGlobalValue();

        internal GridColumnProperty()
            : base(PropertyNames.GridColumn)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
