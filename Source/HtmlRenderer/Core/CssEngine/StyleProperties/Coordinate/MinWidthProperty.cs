namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MinWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

        internal MinWidthProperty()
            : base(PropertyNames.MinWidth, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}