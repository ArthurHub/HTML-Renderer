namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid</c>, CSS Grid 2 §7.8.</summary>
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
