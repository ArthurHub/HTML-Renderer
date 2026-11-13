namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarkerEndProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.MarkerConverter.OrDefault();

        internal MarkerEndProperty()
            : base(PropertyNames.MarkerEnd, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
