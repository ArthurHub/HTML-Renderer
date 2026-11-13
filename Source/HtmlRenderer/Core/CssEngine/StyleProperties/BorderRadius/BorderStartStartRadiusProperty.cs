namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderStartStartRadiusProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.BorderRadiusConverter.OrDefault(Length.Zero);

        internal BorderStartStartRadiusProperty()
            : base(PropertyNames.BorderStartStartRadius, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
