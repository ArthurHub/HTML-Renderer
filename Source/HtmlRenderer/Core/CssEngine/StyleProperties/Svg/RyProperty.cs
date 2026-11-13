namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class RyProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.AutoNonNegativeLengthOrPercentConverter.OrDefault();

        internal RyProperty()
            : base(PropertyNames.Ry, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
