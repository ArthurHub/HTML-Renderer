namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ListStyleImageProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.OptionalImageSourceConverter.OrDefault();

        internal ListStyleImageProperty()
            : base(PropertyNames.ListStyleImage, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}