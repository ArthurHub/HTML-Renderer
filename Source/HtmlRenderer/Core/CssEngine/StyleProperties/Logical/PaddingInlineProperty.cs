namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PaddingInlineProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.Periodic(
                PropertyNames.PaddingInlineStart, PropertyNames.PaddingInlineEnd)
            .OrDefault(Length.Zero);

        internal PaddingInlineProperty()
            : base(PropertyNames.PaddingInline)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
