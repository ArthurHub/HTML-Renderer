namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderImageOutsetProperty : Property
    {
        internal static readonly IValueConverter TheConverter = Converters.LengthOrPercentConverter.Periodic();
        private static readonly IValueConverter StyleConverter = TheConverter.OrDefault(Length.Zero);

        internal BorderImageOutsetProperty()
            : base(PropertyNames.BorderImageOutset)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}