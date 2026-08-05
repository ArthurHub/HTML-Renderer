namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class LeftProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

        internal LeftProperty()
            : base(PropertyNames.Left, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}