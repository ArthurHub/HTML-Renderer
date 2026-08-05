namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderInlineColorProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.Periodic(
            PropertyNames.BorderLeftColor, PropertyNames.BorderRightColor).OrDefault();

        internal BorderInlineColorProperty()
            : base(PropertyNames.BorderInlineColor, PropertyFlags.Hashless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
