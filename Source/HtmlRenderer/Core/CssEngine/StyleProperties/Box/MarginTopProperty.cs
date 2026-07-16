namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarginTopProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

        internal MarginTopProperty()
            : base(PropertyNames.MarginTop, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}