namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ColumnRuleStyleProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.LineStyleConverter.OrDefault(LineStyle.None);

        internal ColumnRuleStyleProperty()
            : base(PropertyNames.ColumnRuleStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}