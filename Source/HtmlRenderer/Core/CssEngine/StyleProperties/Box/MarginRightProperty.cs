namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarginRightProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

        internal MarginRightProperty()
            : base(PropertyNames.MarginRight, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}