namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarkerProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.MarkerConverter.OrDefault();

        internal MarkerProperty()
            : base(PropertyNames.Marker, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
