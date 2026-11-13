namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarginBlockEndProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

        internal MarginBlockEndProperty()
            : base(PropertyNames.MarginBlockEnd, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
