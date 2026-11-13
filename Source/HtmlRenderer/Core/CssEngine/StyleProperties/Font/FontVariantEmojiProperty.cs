namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontVariantEmojiProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new FontGrammarValueConverter(FontVariantGrammar.TryParseEmoji).OrGlobalValue();

        internal FontVariantEmojiProperty()
            : base(PropertyNames.FontVariantEmoji, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
