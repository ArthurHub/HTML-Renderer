namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ColorInterpolationProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.ColorInterpolationConverter.OrDefault();

        internal ColorInterpolationProperty()
            : base(PropertyNames.ColorInterpolation, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
