namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderBottomStyleProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.LineStyleConverter.OrDefault(LineStyle.None);

        internal BorderBottomStyleProperty()
            : base(PropertyNames.BorderBottomStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}