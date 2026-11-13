namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class RxProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.AutoNonNegativeLengthOrPercentConverter.OrDefault();

        internal RxProperty()
            : base(PropertyNames.Rx, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
