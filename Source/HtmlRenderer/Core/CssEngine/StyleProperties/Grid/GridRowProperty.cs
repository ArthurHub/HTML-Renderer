namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-row</c> shorthand (CSS Grid §8.3.1): <c>&lt;grid-line&gt; [ / &lt;grid-line&gt; ]?</c>,
    /// expanding to <c>grid-row-start</c> / <c>grid-row-end</c>.
    /// </summary>
    internal sealed class GridRowProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.GridRowConverter.OrGlobalValue();

        internal GridRowProperty()
            : base(PropertyNames.GridRow)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
