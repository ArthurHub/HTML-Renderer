namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-column</c>, CSS Grid 2 §8.4.</summary>
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
