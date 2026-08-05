namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ContainerTypeProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.ContainerTypeConverter.OrDefault(Keywords.Normal);

        internal ContainerTypeProperty()
            : base(PropertyNames.ContainerType)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}