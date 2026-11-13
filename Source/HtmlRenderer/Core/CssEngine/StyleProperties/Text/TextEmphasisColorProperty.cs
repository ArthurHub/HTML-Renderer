namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextEmphasisColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.ColorConverter.OrDefault();

        internal TextEmphasisColorProperty()
            : base(PropertyNames.TextEmphasisColor, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
