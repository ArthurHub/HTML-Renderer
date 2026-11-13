namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CaretColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CurrentColorConverter.OrAuto().OrDefault();

        internal CaretColorProperty()
            : base(PropertyNames.CaretColor, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
