namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class WordBreakProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.WordBreakConverter;

        public WordBreakProperty()
            : base(PropertyNames.WordBreak, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}