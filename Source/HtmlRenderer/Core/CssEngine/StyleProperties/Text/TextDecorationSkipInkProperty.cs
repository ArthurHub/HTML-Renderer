namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextDecorationSkipInkProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            TokenGrammarValueConverter.OneOf(Keywords.Auto, Keywords.None, Keywords.All).OrDefault();

        internal TextDecorationSkipInkProperty()
            : base(PropertyNames.TextDecorationSkipInk, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
