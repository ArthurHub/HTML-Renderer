namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BaselineShiftProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.BaselineShiftConverter.OrDefault();

        internal BaselineShiftProperty()
            : base(PropertyNames.BaselineShift, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
