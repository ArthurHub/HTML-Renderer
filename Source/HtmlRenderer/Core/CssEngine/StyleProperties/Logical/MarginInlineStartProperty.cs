namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MarginInlineStartProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Length.Zero);

        internal MarginInlineStartProperty()
            : base(PropertyNames.MarginInlineStart, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
