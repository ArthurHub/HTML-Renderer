namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarginInlineEndProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

        internal MarginInlineEndProperty()
            : base(PropertyNames.MarginInlineEnd, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
