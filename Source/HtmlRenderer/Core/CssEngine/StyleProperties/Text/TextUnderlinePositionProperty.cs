namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextUnderlinePositionProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new TokenGrammarValueConverter(TextUnderlinePositionGrammar.TryParse).OrDefault();

        internal TextUnderlinePositionProperty()
            : base(PropertyNames.TextUnderlinePosition, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
