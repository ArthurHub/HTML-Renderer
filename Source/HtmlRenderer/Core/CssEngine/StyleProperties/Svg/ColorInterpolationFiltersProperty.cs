namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ColorInterpolationFiltersProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.ColorInterpolationConverter.OrDefault();

        internal ColorInterpolationFiltersProperty()
            : base(PropertyNames.ColorInterpolationFilters, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
