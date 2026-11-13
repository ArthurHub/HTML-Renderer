namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BackgroundClipProperty : Property
    {
        private static readonly IValueConverter ListConverter =
            Converters.BackgroundClipConverter.FromList().OrDefault(BoxModel.BorderBox);

        internal BackgroundClipProperty()
            : base(PropertyNames.BackgroundClip)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}