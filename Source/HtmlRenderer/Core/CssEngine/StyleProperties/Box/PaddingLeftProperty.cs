namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PaddingLeftProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

        internal PaddingLeftProperty()
            : base(PropertyNames.PaddingLeft, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}