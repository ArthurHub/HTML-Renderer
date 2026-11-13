namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class DProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.PathDataConverter.OrDefault();

        internal DProperty()
            : base(PropertyNames.D, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
