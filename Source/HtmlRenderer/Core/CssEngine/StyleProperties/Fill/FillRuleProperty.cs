namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FillRuleProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.FillRuleConverter.OrDefault(FillRule.Nonzero);

        public FillRuleProperty()
            : base(PropertyNames.FillRule, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}