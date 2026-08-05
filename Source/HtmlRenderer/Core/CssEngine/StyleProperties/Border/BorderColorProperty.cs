namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderColorProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.Periodic(
            PropertyNames.BorderTopColor, PropertyNames.BorderRightColor, PropertyNames.BorderBottomColor,
            PropertyNames.BorderLeftColor).OrDefault();

        internal BorderColorProperty()
            : base(PropertyNames.BorderColor, PropertyFlags.Hashless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}