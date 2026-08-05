namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PaddingTopProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

        internal PaddingTopProperty()
            : base(PropertyNames.PaddingTop, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}