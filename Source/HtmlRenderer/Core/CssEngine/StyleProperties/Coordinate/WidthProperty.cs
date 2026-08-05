namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class WidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

        internal WidthProperty()
            : base(PropertyNames.Width, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}