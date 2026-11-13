namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PaintOrderProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.PaintOrderConverter.OrDefault();

        internal PaintOrderProperty()
            : base(PropertyNames.PaintOrder, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
