namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarginLeftProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

        internal MarginLeftProperty()
            : base(PropertyNames.MarginLeft, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}