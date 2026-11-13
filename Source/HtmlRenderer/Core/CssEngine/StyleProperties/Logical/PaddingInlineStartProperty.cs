namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PaddingInlineStartProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

        internal PaddingInlineStartProperty()
            : base(PropertyNames.PaddingInlineStart, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
