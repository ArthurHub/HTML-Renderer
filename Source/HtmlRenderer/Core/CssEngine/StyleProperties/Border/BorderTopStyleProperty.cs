namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderTopStyleProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.LineStyleConverter.OrDefault(LineStyle.None);

        internal BorderTopStyleProperty()
            : base(PropertyNames.BorderTopStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}