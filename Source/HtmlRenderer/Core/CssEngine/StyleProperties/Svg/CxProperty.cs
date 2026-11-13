namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CxProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.OrDefault();

        internal CxProperty()
            : base(PropertyNames.Cx, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
