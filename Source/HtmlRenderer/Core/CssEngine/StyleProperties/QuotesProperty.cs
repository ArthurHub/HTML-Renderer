namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class QuotesProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.EvenStringsConverter.OrNone().OrDefault(new[] { "«", "»" });

        internal QuotesProperty()
            : base(PropertyNames.Quotes, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}