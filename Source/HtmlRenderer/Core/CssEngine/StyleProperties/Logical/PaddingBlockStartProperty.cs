namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PaddingBlockStartProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

        internal PaddingBlockStartProperty()
            : base(PropertyNames.PaddingBlockStart, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
