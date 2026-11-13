namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontSizeAdjustProperty : Property
    {
        private static readonly IValueConverter StyleConverter = FontSizeAdjustGrammar.Converter.OrDefault();

        internal FontSizeAdjustProperty()
            : base(PropertyNames.FontSizeAdjust, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}