namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarginInlineProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.Periodic(
                PropertyNames.MarginLeft, PropertyNames.MarginRight)
            .OrDefault(Length.Zero);

        internal MarginInlineProperty()
            : base(PropertyNames.MarginInline)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
