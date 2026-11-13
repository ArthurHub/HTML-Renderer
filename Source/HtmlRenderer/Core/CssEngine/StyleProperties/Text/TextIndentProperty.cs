namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextIndentProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new TokenGrammarValueConverter(TextIndentGrammar.TryParse).OrDefault(Length.Zero);

        internal TextIndentProperty()
            : base(PropertyNames.TextIndent, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}