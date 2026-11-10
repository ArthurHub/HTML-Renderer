namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-row</c>, CSS Grid 2 §8.4.</summary>
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
