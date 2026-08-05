namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class WhiteSpaceProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.WhitespaceConverter.OrDefault(Whitespace.Normal);

        internal WhiteSpaceProperty()
            : base(PropertyNames.WhiteSpace, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}