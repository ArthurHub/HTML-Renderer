namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderInlineColorProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.Periodic(
                PropertyNames.BorderInlineStartColor, PropertyNames.BorderInlineEndColor)
            .OrDefault();

        internal BorderInlineColorProperty()
            : base(PropertyNames.BorderInlineColor, PropertyFlags.Hashless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
