namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-area</c>, CSS Grid 2 §8.4.</summary>
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
