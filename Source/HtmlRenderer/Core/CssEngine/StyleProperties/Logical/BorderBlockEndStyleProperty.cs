namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBlockEndStyleProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LineStyleConverter.OrDefault(LineStyle.None);

        internal BorderBlockEndStyleProperty()
            : base(PropertyNames.BorderBlockEndStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
