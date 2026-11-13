namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ClipRuleProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.FillRuleConverter.OrDefault();

        internal ClipRuleProperty()
            : base(PropertyNames.ClipRule, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
