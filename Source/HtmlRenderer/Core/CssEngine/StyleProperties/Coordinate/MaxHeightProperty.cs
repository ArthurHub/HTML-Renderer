namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MaxHeightProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.OptionalLengthOrPercentConverter.OrDefault();

        internal MaxHeightProperty()
            : base(PropertyNames.MaxHeight, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}