namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-area</c> shorthand (CSS Grid §8.3.1): <c>&lt;grid-line&gt; [ / &lt;grid-line&gt; ]{0,3}</c>,
    /// expanding to <c>grid-row-start</c> / <c>grid-column-start</c> / <c>grid-row-end</c> /
    /// <c>grid-column-end</c>. The single named-area form is out of v1 scope.
    /// </summary>
    internal sealed class GridAreaProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.GridAreaConverter.OrGlobalValue();

        internal GridAreaProperty()
            : base(PropertyNames.GridArea)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
