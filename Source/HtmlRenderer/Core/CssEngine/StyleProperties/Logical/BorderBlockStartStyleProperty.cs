namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBlockStartStyleProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LineStyleConverter.OrDefault(LineStyle.None);

        internal BorderBlockStartStyleProperty()
            : base(PropertyNames.BorderBlockStartStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
