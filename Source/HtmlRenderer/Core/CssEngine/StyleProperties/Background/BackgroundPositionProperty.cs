namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BackgroundPositionProperty : Property
    {
        private static readonly IValueConverter ListConverter =
            Converters.PointConverter.FromList().OrDefault(Point.Center);

        internal BackgroundPositionProperty()
            : base(PropertyNames.BackgroundPosition, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}