namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PaddingBlockProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.Periodic(
                PropertyNames.PaddingTop, PropertyNames.PaddingBottom)
            .OrDefault(Length.Zero);

        internal PaddingBlockProperty()
            : base(PropertyNames.PaddingBlock)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
