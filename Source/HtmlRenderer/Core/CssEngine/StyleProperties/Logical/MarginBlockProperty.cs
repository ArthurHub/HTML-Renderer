namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarginBlockProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.Periodic(
                PropertyNames.MarginTop, PropertyNames.MarginBottom)
            .OrDefault(Length.Zero);

        internal MarginBlockProperty()
            : base(PropertyNames.MarginBlock)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
