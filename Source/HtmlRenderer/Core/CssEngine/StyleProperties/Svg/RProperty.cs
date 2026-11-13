namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class RProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.NonNegativeLengthOrPercentConverter.OrDefault();

        internal RProperty()
            : base(PropertyNames.R, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
