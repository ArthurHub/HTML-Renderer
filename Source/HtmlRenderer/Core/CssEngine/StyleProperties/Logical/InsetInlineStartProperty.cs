namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class InsetInlineStartProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

        internal InsetInlineStartProperty()
            : base(PropertyNames.InsetInlineStart, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
