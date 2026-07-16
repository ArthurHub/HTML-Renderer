namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TopProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

        internal TopProperty()
            : base(PropertyNames.Top, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}