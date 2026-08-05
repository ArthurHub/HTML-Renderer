namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PaddingRightProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

        internal PaddingRightProperty()
            : base(PropertyNames.PaddingRight, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}