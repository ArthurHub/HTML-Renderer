namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextEmphasisPositionProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new TokenGrammarValueConverter(TextEmphasisGrammar.TryParsePosition).OrDefault();

        internal TextEmphasisPositionProperty()
            : base(PropertyNames.TextEmphasisPosition, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
