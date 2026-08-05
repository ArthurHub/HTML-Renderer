namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderRightStyleProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.LineStyleConverter.OrDefault(LineStyle.None);

        internal BorderRightStyleProperty()
            : base(PropertyNames.BorderRightStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}