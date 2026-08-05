namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PaddingBottomProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

        internal PaddingBottomProperty()
            : base(PropertyNames.PaddingBottom, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}