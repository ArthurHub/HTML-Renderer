namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ColumnRuleWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LineWidthConverter.OrDefault(Length.Medium);

        internal ColumnRuleWidthProperty()
            : base(PropertyNames.ColumnRuleWidth, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}