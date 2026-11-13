namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextEmphasisStyleProperty : Property
    {
        internal static readonly IValueConverter ValueConverter =
            new TokenGrammarValueConverter(TextEmphasisGrammar.TryParseStyle);

        private static readonly IValueConverter StyleConverter = ValueConverter.OrDefault();

        internal TextEmphasisStyleProperty()
            : base(PropertyNames.TextEmphasisStyle, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
