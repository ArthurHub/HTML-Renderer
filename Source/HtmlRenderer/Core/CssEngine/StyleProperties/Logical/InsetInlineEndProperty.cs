namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class InsetInlineEndProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

        internal InsetInlineEndProperty()
            : base(PropertyNames.InsetInlineEnd, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
