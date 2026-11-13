namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StopColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.OrDefault();

        internal StopColorProperty()
            : base(PropertyNames.StopColor, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
