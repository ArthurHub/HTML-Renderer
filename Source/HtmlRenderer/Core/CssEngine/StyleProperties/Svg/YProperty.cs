namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class YProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.OrDefault();

        internal YProperty()
            : base(PropertyNames.Y, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
