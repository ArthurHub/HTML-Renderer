namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarkerMidProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.MarkerConverter.OrDefault();

        internal MarkerMidProperty()
            : base(PropertyNames.MarkerMid, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
