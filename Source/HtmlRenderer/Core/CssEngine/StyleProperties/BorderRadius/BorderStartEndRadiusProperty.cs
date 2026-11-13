namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderStartEndRadiusProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.BorderRadiusConverter.OrDefault(Length.Zero);

        internal BorderStartEndRadiusProperty()
            : base(PropertyNames.BorderStartEndRadius, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
