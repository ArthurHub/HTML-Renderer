namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PerspectiveProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthConverter.OrNone().OrDefault(Length.Zero);

        internal PerspectiveProperty()
            : base(PropertyNames.Perspective, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}