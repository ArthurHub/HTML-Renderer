namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BackgroundSizeProperty : Property
    {
        private static readonly IValueConverter ListConverter =
            Converters.BackgroundSizeConverter.FromList().OrDefault();

        internal BackgroundSizeProperty()
            : base(PropertyNames.BackgroundSize, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}