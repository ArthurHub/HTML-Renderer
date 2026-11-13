namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarginBlockStartProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

        internal MarginBlockStartProperty()
            : base(PropertyNames.MarginBlockStart, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
