namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarginBottomProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

        internal MarginBottomProperty()
            : base(PropertyNames.MarginBottom, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}