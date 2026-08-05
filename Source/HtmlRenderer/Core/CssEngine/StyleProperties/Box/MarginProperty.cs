namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarginProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.Periodic(
                PropertyNames.MarginTop, PropertyNames.MarginRight, PropertyNames.MarginBottom,
                PropertyNames.MarginLeft)
            .OrDefault(Length.Zero);

        internal MarginProperty()
            : base(PropertyNames.Margin)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}