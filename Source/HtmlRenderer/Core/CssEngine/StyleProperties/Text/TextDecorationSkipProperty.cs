namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextDecorationSkipProperty : Property
    {
        // Legacy (CSS Text Decoration 3) skip list; the initial value is objects.
        private static readonly IValueConverter StyleConverter =
            new TokenGrammarValueConverter(TextEmphasisGrammar.TryParseSkip).OrDefault();

        internal TextDecorationSkipProperty()
            : base(PropertyNames.TextDecorationSkip)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
