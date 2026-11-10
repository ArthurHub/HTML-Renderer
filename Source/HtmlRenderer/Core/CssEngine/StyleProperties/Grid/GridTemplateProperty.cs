namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-template</c>, CSS Grid 2 §7.4.</summary>
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
