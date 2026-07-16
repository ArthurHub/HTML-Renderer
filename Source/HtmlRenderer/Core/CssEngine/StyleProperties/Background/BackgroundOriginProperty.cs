namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BackgroundOriginProperty : Property
    {
        private static readonly IValueConverter ListConverter =
            Converters.BoxModelConverter.FromList().OrDefault(BoxModel.PaddingBox);

        internal BackgroundOriginProperty()
            : base(PropertyNames.BackgroundOrigin)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}