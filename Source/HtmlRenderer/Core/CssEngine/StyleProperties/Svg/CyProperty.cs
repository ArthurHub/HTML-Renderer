namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CyProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.OrDefault();

        internal CyProperty()
            : base(PropertyNames.Cy, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
