namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MinHeightProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

        internal MinHeightProperty()
            : base(PropertyNames.MinHeight, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}