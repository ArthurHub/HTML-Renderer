namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ListStyleTypeProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.ListStyleConverter.OrDefault(ListStyle.Disc);

        internal ListStyleTypeProperty()
            : base(PropertyNames.ListStyleType, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}