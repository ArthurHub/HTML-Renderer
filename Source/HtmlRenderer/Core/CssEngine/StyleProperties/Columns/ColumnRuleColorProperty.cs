namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ColumnRuleColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.ColorConverter.OrDefault(Color.Transparent);

        internal ColumnRuleColorProperty()
            : base(PropertyNames.ColumnRuleColor, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}