namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBlockColorProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.Periodic(
            PropertyNames.BorderTopColor, PropertyNames.BorderBottomColor).OrDefault();

        internal BorderBlockColorProperty()
            : base(PropertyNames.BorderBlockColor, PropertyFlags.Hashless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
