namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarkerStartProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.MarkerConverter.OrDefault();

        internal MarkerStartProperty()
            : base(PropertyNames.MarkerStart, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
