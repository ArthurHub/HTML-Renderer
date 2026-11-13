namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PaddingInlineEndProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

        internal PaddingInlineEndProperty()
            : base(PropertyNames.PaddingInlineEnd, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
