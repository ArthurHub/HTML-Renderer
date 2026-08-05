namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BottomProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

        internal BottomProperty()
            : base(PropertyNames.Bottom, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}