namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BackgroundAttachmentProperty : Property
    {
        private static readonly IValueConverter AttachmentConverter =
            Converters.BackgroundAttachmentConverter.FromList().OrDefault(BackgroundAttachment.Scroll);

        internal BackgroundAttachmentProperty()
            : base(PropertyNames.BackgroundAttachment)
        {
        }

        internal override IValueConverter Converter => AttachmentConverter;
    }
}