namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PdfTagTypeProperty : Property
    {
        internal PdfTagTypeProperty() : base(PropertyNames.PdfTagType)
        {
        }

        internal override IValueConverter Converter => Converters.PdfTagTypeConverter.OrDefault();
    }
}
