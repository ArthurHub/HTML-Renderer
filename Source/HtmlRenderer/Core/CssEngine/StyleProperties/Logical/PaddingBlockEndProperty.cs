namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PaddingBlockEndProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

        internal PaddingBlockEndProperty()
            : base(PropertyNames.PaddingBlockEnd, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
