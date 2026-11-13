namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class XProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.OrDefault();

        internal XProperty()
            : base(PropertyNames.X, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
