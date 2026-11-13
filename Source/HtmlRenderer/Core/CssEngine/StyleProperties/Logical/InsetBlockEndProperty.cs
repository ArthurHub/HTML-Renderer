namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class InsetBlockEndProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

        internal InsetBlockEndProperty()
            : base(PropertyNames.InsetBlockEnd, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
