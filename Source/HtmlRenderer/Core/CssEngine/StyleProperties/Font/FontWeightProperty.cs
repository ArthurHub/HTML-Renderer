namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class FontWeightProperty : Property
    {
        private static readonly IValueConverter StyleConverter = FontWeightConverter.Or(
            WeightIntegerConverter).OrDefault(FontWeight.Normal);

        internal FontWeightProperty()
            : base(PropertyNames.FontWeight, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}